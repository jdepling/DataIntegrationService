using System.Text;
using Integration.Data;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Integration.OutboxPublisher.Services
{
    public class PublishService : IPublishService
    {
        private readonly RabbitMqOptions _rabbitMqOptions;

        public PublishService(IOptions<RabbitMqOptions> rabbitMqOptions)
        {
            _rabbitMqOptions = rabbitMqOptions.Value ?? throw new ArgumentNullException(nameof(rabbitMqOptions));
        }
        public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqOptions.Host,
                Port     = _rabbitMqOptions.Port,
                UserName = _rabbitMqOptions.Username,
                Password = _rabbitMqOptions.Password
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            var body = Encoding.UTF8.GetBytes(message.Payload);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: _rabbitMqOptions.QueueName,
                body: body);
        }
    }
}
