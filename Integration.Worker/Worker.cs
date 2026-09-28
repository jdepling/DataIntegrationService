using System.Text;
using Integration.Worker.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using Integration.Worker.Models;

namespace Integration.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMqOptions _rabbitMqOptions;
        private readonly ISystemBClient _systemBClient;

        public Worker(IOptions<RabbitMqOptions> rabbitMqOptions, ISystemBClient systemBClient, ILogger<Worker> logger)
        {
            _logger          = logger ?? throw new ArgumentNullException(nameof(logger));
            _rabbitMqOptions = rabbitMqOptions.Value ?? throw new ArgumentNullException(nameof(rabbitMqOptions));
            _systemBClient   = systemBClient ?? throw new ArgumentNullException(nameof(systemBClient));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqOptions.Host,
                Port = _rabbitMqOptions.Port,
                UserName = _rabbitMqOptions.Username,
                Password = _rabbitMqOptions.Password
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                try
                {
                    var body = args.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);

                    var integrationMessage =
                        JsonSerializer.Deserialize<IntegrationMessage>(messageJson);

                    _logger.LogInformation(
                        "Raw RabbitMQ message: {MessageJson}",
                        messageJson);

                    _logger.LogInformation(
                        "SourceId: {SourceId}, Data kind: {DataKind}, Data: {Data}",
                        integrationMessage.SourceId,
                        integrationMessage.Data.ValueKind,
                        integrationMessage.Data);

                    if (integrationMessage == null)
                    {
                        throw new InvalidOperationException(
                            "Unable to deserialize integration message.");
                    }

                    var systemBRequest = new SystemBMessageRequest
                    {
                        SourceId = integrationMessage.SourceId,
                        CustomerId = integrationMessage.Data
                            .GetProperty("customerId")
                            .GetInt32(),
                        Name = integrationMessage.Data
                            .GetProperty("name")
                            .GetString() ?? string.Empty,
                        Amount = integrationMessage.Data
                            .GetProperty("amount")
                            .GetDecimal(),
                        Status = integrationMessage.Data
                            .GetProperty("status")
                            .GetString() ?? string.Empty
                    };

                    _logger.LogInformation("System B request: {Request}", JsonSerializer.Serialize(systemBRequest));

                    await _systemBClient.SendAsync(
                        systemBRequest,
                        stoppingToken);

                    await channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false);

                    _logger.LogInformation(
                        "Message {SourceId} successfully sent to System B.",
                        integrationMessage.SourceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error processing RabbitMQ message.");
                }
            };

            await channel.BasicConsumeAsync(
                queue: _rabbitMqOptions.QueueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Worker is listening for messages...");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
