using Integration.Admin.Models;

namespace Integration.Admin.Services
{
    public interface IAdminApiClient
    {
        Task<List<FailedMessageViewModel>> GetFailedMessagesAsync();
        Task<bool> ReplayMessageAsync(Guid id);
    }
}
