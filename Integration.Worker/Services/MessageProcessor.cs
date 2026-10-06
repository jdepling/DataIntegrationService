using System.Text.Json;
using Integration.Worker.Models;

namespace Integration.Worker.Services
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly ILogger<MessageProcessor> _logger;
        private readonly ISystemBClient _systemBClient;

        public MessageProcessor(ILogger<MessageProcessor> logger, ISystemBClient systemBClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _systemBClient = systemBClient ?? throw new ArgumentNullException(nameof(systemBClient));
        }

        public async Task ProcessAsync(IntegrationMessage integrationMessage, string messageJson, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Raw RabbitMQ message: {MessageJson}",
                messageJson);

            _logger.LogInformation(
                "SourceId: {SourceId}, Data kind: {DataKind}, Data: {Data}",
                integrationMessage.SourceId,
                integrationMessage.Data.ValueKind,
                integrationMessage.Data);

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

            _logger.LogInformation(
                "System B request: {Request}",
                JsonSerializer.Serialize(systemBRequest));

            await _systemBClient.SendAsync(
                systemBRequest,
                cancellationToken);

            _logger.LogInformation(
                "Message {SourceId} successfully sent to System B.",
                integrationMessage.SourceId);
        }
    }
}