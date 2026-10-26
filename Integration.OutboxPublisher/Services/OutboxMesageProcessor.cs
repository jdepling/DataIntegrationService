namespace Integration.OutboxPublisher.Services
{
    public class OutboxMessageProcessor : IOutboxMessageProcessor
    {
        private readonly IOutboxService _outboxService;
        private readonly IPublishService _publishService;
        private readonly ILogger<OutboxMessageProcessor> _logger;

        public OutboxMessageProcessor(IOutboxService outboxService, IPublishService publishService, ILogger<OutboxMessageProcessor> logger)
        {
            _outboxService = outboxService ?? throw new ArgumentNullException(nameof(outboxService));
            _publishService = publishService ?? throw new ArgumentNullException(nameof(publishService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Retrieves unpublished messages, publishes them, and marks them as published.
        /// </summary>
        public async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            var messages = await _outboxService.GetUnpublishedMessagesAsync(cancellationToken);

            foreach (var message in messages)
            {
                _logger.LogInformation("Found unpublished message {MessageId} from source {SourceId}", message.Id, message.SourceId);

                await _publishService.PublishAsync(message, cancellationToken);

                await _outboxService.MarkMessageAsPublishedAsync(message.Id, cancellationToken);
            }
        }
    }
}
