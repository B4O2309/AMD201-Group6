using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace URLService.RabbitMQ
{
    public class UrlConsumer : BackgroundService
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string _queueName = "user_events";
        private readonly IConfiguration _configuration;
        private readonly ILogger<UrlConsumer> _logger;

        public UrlConsumer(IConfiguration configuration, ILogger<UrlConsumer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Retry loop — keeps trying until RabbitMQ is ready
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _configuration["RabbitMQ:Host"] ?? "rabbitmq",
                        UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                        Password = _configuration["RabbitMQ:Password"] ?? "guest"
                    };

                    _connection = await factory.CreateConnectionAsync(stoppingToken);
                    _channel = await _connection.CreateChannelAsync(null, stoppingToken);

                    await _channel.QueueDeclareAsync(
                        queue: _queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: stoppingToken);

                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation("[URLService] Received user event: {Message}", message);
                        await Task.CompletedTask;
                    };

                    await _channel.BasicConsumeAsync(
                        queue: _queueName,
                        autoAck: true,
                        consumer: consumer,
                        cancellationToken: stoppingToken);

                    _logger.LogInformation("[UrlConsumer] Connected to RabbitMQ, listening on queue: {Queue}", _queueName);

                    // Keep running until cancelled
                    while (!stoppingToken.IsCancellationRequested)
                        await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown — exit loop
                    break;
                }
                catch (Exception ex)
                {
                    // RabbitMQ not ready yet — retry after 5 seconds
                    _logger.LogWarning("[UrlConsumer] RabbitMQ not ready, retrying in 5s. Error: {Error}", ex.Message);
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}