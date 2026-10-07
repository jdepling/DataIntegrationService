using Integration.Admin.Models;

namespace Integration.Admin.Services
{
    public class AdminApiClient : IAdminApiClient
    {
        private readonly HttpClient _httpClient;

        public AdminApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<FailedMessageViewModel>> GetFailedMessagesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<FailedMessageViewModel>>(
                "api/admin/messages") ?? new List<FailedMessageViewModel>();
        }

        public async Task<FailedMessageViewModel?> GetFailedMessageAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<FailedMessageViewModel>(
                $"api/admin/messages/{id}");
        }

        public async Task<bool> ReplayMessageAsync(Guid id)
        {
            var response = await _httpClient.PostAsync(
                $"api/admin/messages/{id}/replay",
                null);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ReplayFixedMessageAsync(Guid id, string payload)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/admin/messages/{id}/replay-fixed",
                payload);

            return response.IsSuccessStatusCode;
        }
    }
}
