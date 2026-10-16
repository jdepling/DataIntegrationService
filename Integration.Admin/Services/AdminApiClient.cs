using Integration.Admin.Models;

namespace Integration.Admin.Services
{
    /// <summary>
    ///     Provides HTTP client functionality to interact with the Admin API.
    ///     Handles operations related to failed messages, including retrieval, replay, and replay with fixed payloads.
    /// </summary>
    public class AdminApiClient : IAdminApiClient
    {
        private readonly IHttpClientAdapter _httpClient;

        public AdminApiClient(IHttpClientAdapter httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        ///     Retrieves all failed messages from the admin API.
        /// </summary>
        /// <returns>
        ///     A list of failed messages, or an empty list if none are found.
        /// </returns>
        public async Task<List<FailedMessageViewModel>> GetFailedMessagesAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<List<FailedMessageViewModel>>("api/admin/messages");
            return result ?? new List<FailedMessageViewModel>();
        }

        /// <summary>
        ///     Retrieves a specific failed message by its unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the failed message to retrieve.
        /// </param>
        /// <returns>
        ///     The failed message if found; otherwise, null.
        /// </returns>
        public async Task<FailedMessageViewModel?> GetFailedMessageAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<FailedMessageViewModel>(
                $"api/admin/messages/{id}", CancellationToken.None);
        }

        /// <summary>
        ///     Replays a failed message using its original payload.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the message to replay.
        /// </param>
        /// <returns>
        ///     True if the replay was successful; otherwise, false.
        /// </returns>
        public async Task<bool> ReplayMessageAsync(Guid id)
        {
            var response = await _httpClient.PostAsync(
                $"api/admin/messages/{id}/replay",
                null);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        ///     Replays a failed message with a corrected payload.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the message to replay.
        /// </param>
        /// <param name="payload">
        ///     The corrected message payload to send.
        /// </param>
        /// <returns>
        ///     True if the replay was successful; otherwise, false.
        /// </returns>
        public async Task<bool> ReplayFixedMessageAsync(Guid id, string payload)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/admin/messages/{id}/replay-fixed",
                payload);

            return response.IsSuccessStatusCode;
        }
    }
}
