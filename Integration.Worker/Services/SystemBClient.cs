using System.Net.Http.Json;
using Integration.Worker.Models;

namespace Integration.Worker.Services
{
    public class SystemBClient : ISystemBClient
    {
        private readonly HttpClient _httpClient;

        public SystemBClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task SendAsync(SystemBMessageRequest request, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/Messages",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }
    }
}