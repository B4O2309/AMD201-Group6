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
        private readonly string _exchangeName = "user-event";
        private readonly IConfiguration _configuration;
        private readonly ILogger<UrlConsumer> _logger;

        public UrlConsumer(IConfiguration configuration, ILogger<UrlConsumer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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

                    await _channel.ExchangeDeclareAsync(
                        exchange: _exchangeName,
                        type: ExchangeType.Fanout, // Using Fanout so multiple services can subscribe
                        durable: true,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: stoppingToken);

                    // 2. Declare the Queue
                    await _channel.QueueDeclareAsync(
                        queue: _queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: stoppingToken);

                    await _channel.QueueBindAsync(
                        queue: _queueName,
                        exchange: _exchangeName,
                        routingKey: string.Empty,
                        cancellationToken: stoppingToken);

                    _logger.LogInformation("[UrlConsumer] Bound Queue {Queue} to Exchange {Exchange}", _queueName, _exchangeName);

                    // 4. Set up Consumer logic
                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        _logger.LogInformation("[URLService] >>> RECEIVED EVENT: {Message}", message);

                        await Task.CompletedTask;
                    };

                    await _channel.BasicConsumeAsync(
                        queue: _queueName,
                        autoAck: true,
                        consumer: consumer,
                        cancellationToken: stoppingToken);

                    _logger.LogInformation("[UrlConsumer] Listening for messages...");

                    // Keep the background task alive
                    while (!stoppingToken.IsCancellationRequested)
                        await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[UrlConsumer] Connection failed, retrying in 5s... Error: {Error}", ex.Message);
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