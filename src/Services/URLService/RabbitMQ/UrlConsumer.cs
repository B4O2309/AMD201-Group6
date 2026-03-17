using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace URLService.RabbitMQ
{
    public class UrlConsumer : BackgroundService
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string _queueName = "user_events";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 1. Setup connection factory
            var factory = new ConnectionFactory { HostName = "localhost" }; // Use "rabbitmq" for Docker environments

            // 2. Create Connection and Channel asynchronously
            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(null, stoppingToken);

            // 3. Declare the queue
            await _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            // 4. Setup the Consumer
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Process the received message
                Console.WriteLine($"[URLService] Received message: {message}");

                await Task.CompletedTask;
            };

            // 5. Start consuming
            await _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: true,
                consumer: consumer,
                cancellationToken: stoppingToken);

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async void Dispose()
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
            base.Dispose();
        }
    }
}   