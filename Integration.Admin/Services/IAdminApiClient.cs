using Integration.Admin.Models;

namespace Integration.Admin.Services
{
    public interface IAdminApiClient
    {
        Task<List<FailedMessageViewModel>> GetFailedMessagesAsync();
        Task<FailedMessageViewModel?> GetFailedMessageAsync(Guid id);
        Task<bool> ReplayMessageAsync(Guid id);
        Task<bool> ReplayFixedMessageAsync(Guid id, string payload);
    }
}
