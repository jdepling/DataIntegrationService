using Integration.Data;

namespace Integration.Api.Services
{
    public interface IReplayMessageService
    {
        Task<FailedMessage?> GetFailedMessageAsync(Guid id);
        Task<List<FailedMessage>> GetFailedMessagesAsync();
        Task<Guid?> ReplayAsync(Guid failedMessageId);
        Task<Guid?> ReplayWithPayloadAsync(Guid failedMessageId, string payload);
    }
}