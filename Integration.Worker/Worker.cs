using System.Text;
using System.Text.Json;
using Integration.Data;
using Integration.Worker.Models;
using Integration.Worker.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Integration.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMqOptions _rabbitMqOptions;
        private readonly IMessageProcessor _messageProcessor;
        private readonly IFailedMessageService _failedMessageService;

        public Worker(IOptions<RabbitMqOptions> rabbitMqOptions, IMessageProcessor messageProcessor, IFailedMessageService failedMessageService, ILogger<Worker> logger)
        {
            _logger          = logger ?? throw new ArgumentNullException(nameof(logger));
            _rabbitMqOptions = rabbitMqOptions.Value ?? throw new ArgumentNullException(nameof(rabbitMqOptions));
            _messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
            _failedMessageService = failedMessageService ?? throw new ArgumentNullException(nameof(failedMessageService));
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
                var body = args.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                IntegrationMessage? integrationMessage = null;

                try
                {

                    integrationMessage = JsonSerializer.Deserialize<IntegrationMessage>(messageJson);

                    if (integrationMessage == null)
                    {
                        throw new InvalidOperationException("Unable to deserialize integration message.");
                    }

                    await _messageProcessor.ProcessAsync(
                        integrationMessage,
                        messageJson,
                        stoppingToken);

                    await channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Message processing failed for {SourceId}: {ErrorMessage}", integrationMessage?.SourceId ?? "Unknown", ex.Message);

                    var failedMessage = new FailedMessage
                    {
                        Id = Guid.NewGuid(),
                        SourceId = integrationMessage?.SourceId ?? "Unknown",
                        Payload = messageJson,
                        ErrorMessage = ex.Message,
                        FailureType = DetermineFailureType(ex),
                        CreatedAt = DateTime.UtcNow,
                        LastAttemptAt = DateTime.UtcNow
                    };

                    await _failedMessageService.SaveAsync(
                        failedMessage,
                        stoppingToken);

                    await channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false);

                    _logger.LogInformation(
                        "Failed message {SourceId} saved and acknowledged.",
                        failedMessage.SourceId);
                }
            };

            await channel.BasicConsumeAsync(
                queue: _rabbitMqOptions.QueueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Worker is listening for messages...");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private static FailureType DetermineFailureType(Exception ex)
        {
            if (ex is HttpRequestException httpException)
            {
                if (httpException.StatusCode is null)
                    return FailureType.Transient;

                var statusCode = (int)httpException.StatusCode.Value;

                if (statusCode == 408 || statusCode == 429 || statusCode >= 500)
                    return FailureType.Transient;

                if (statusCode >= 400 && statusCode < 500)
                    return FailureType.Permanent;
            }

            return FailureType.Unknown;
        }
    }
}
