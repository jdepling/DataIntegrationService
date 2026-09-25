using Integration.Data;
using Integration.OutboxPublisher.Services;
using Microsoft.EntityFrameworkCore;

namespace Integration.OutboxPublisher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOutboxService _outboxService;
        private readonly IPublishService _publishService;

        public Worker(IDbContextFactory<IntegrationDbContext> dbContextFactory, IOutboxService outboxService, IPublishService publishService, ILogger<Worker> logger)
        {
            _outboxService = outboxService ?? throw new ArgumentNullException(nameof(outboxService));
            _publishService = publishService ?? throw new ArgumentNullException(nameof(publishService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var messages = await _outboxService.GetUnpublishedMessagesAsync(stoppingToken);

                foreach (var message in messages)
                {
                    _logger.LogInformation(
                        "Found unpublished message {MessageId} from source {SourceId}",
                        message.Id,
                        message.SourceId);

                    // Send message to RabbitMQ
                    await _publishService.PublishAsync(message, stoppingToken);

                    // Mark message as published
                    await _outboxService.MarkMessageAsPublishedAsync(message.Id, stoppingToken);

                    Console.WriteLine($"Published: {message}");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }
    }
}
