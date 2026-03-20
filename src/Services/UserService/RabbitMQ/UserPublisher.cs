using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace UserService.RabbitMQ
{
    /// <summary>
    /// Publishes user lifecycle events to RabbitMQ queue "user_events".
    /// Queue name MUST match exactly what URLService's UrlConsumer listens on.
    /// Registered as Singleton so the connection is reused across requests.
    /// </summary>
    public class UserPublisher : IAsyncDisposable
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string _queueName = "user_events";
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserPublisher> _logger;
        private bool _initialized = false;

        public UserPublisher(IConfiguration configuration, ILogger<UserPublisher> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Lazy init — connect on first publish, not at app startup
        // This prevents startup failures when RabbitMQ container is still booting
        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "rabbitmq",
                UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Queue declaration must match URLService UrlConsumer exactly
            await _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _initialized = true;
            _logger.LogInformation("[UserPublisher] Connected to RabbitMQ, queue: {Queue}", _queueName);
        }

        /// <summary>
        /// Published after a user registers successfully.
        /// URLService logs this; can be extended for per-user URL statistics.
        /// </summary>
        public async Task PublishUserRegisteredAsync(int userId, string email, string username, string role)
        {
            await EnsureInitializedAsync();
            await PublishAsync(new
            {
                EventType = "user_registered",
                UserId = userId,
                Email = email,
                Username = username,
                Role = role,
                Timestamp = DateTime.UtcNow
            });
            _logger.LogInformation("[UserPublisher] Published user_registered for UserId: {UserId}", userId);
        }

        /// <summary>
        /// Published when an admin deletes a user account.
        /// URLService can use this to flag or clean up that user's URLs.
        /// </summary>
        public async Task PublishUserDeletedAsync(int userId, string email)
        {
            await EnsureInitializedAsync();
            await PublishAsync(new
            {
                EventType = "user_deleted",
                UserId = userId,
                Email = email,
                Timestamp = DateTime.UtcNow
            });
            _logger.LogInformation("[UserPublisher] Published user_deleted for UserId: {UserId}", userId);
        }

        private async Task PublishAsync(object payload)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
            var props = new BasicProperties { Persistent = true };

            await _channel!.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _queueName,
                mandatory: false,
                basicProperties: props,
                body: body);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
        }
    }
}