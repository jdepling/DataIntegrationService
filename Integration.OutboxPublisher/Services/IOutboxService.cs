using Integration.Data;

namespace Integration.OutboxPublisher.Services
{
    public interface IOutboxService
    {
        Task<List<OutboxMessage>> GetUnpublishedMessagesAsync(
            CancellationToken cancellationToken);
        Task MarkMessageAsPublishedAsync(Guid messageId, CancellationToken cancellationToken);
    }
}
