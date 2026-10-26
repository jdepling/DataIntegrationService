using Integration.OutboxPublisher.Services;

namespace Integration.OutboxPublisher
{
    public class Worker : BackgroundService
    {
        private readonly IOutboxMessageProcessor _messageProcessor;

        public Worker(IOutboxMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
        }

        /// <summary>
        ///     Executes the background service on a five-second interval.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _messageProcessor.ProcessMessagesAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
