namespace Integration.Api.Services
{
    public interface IReplayMessageService
    {
        Task<Guid?> ReplayAsync(Guid failedMessageId);
    }
}