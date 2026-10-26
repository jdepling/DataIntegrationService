namespace Integration.OutboxPublisher.Services
{
    public interface IOutboxMessageProcessor
    {
        /// <summary>
        ///     Processes unpublished outbox messages.
        /// </summary>
        Task ProcessMessagesAsync(CancellationToken cancellationToken);
    }
}