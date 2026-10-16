namespace Integration.Admin.Services
{
    public interface IHttpClientAdapter
    {
        Task<T?> GetFromJsonAsync<T>(string requestUri);
        Task<T?> GetFromJsonAsync<T>(string requestUri, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent? content);
        Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value);
    }
}
