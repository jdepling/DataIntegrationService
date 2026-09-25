using Integration.Data;

namespace Integration.OutboxPublisher.Services
{
    public interface IPublishService
    {
        Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken);
    }
}