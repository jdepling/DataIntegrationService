using Integration.Data;

namespace Integration.Api.Services
{
    public interface IReplayMessageService
    {
        Task<Guid?> ReplayAsync(Guid failedMessageId);
        Task<List<FailedMessage>> GetFailedMessagesAsync();
    }
}