
using Auth0.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Integration.Admin.Services
{
    /// <summary>
    ///     Provides an adapter for HTTP client operations, implementing the <see cref="IHttpClientAdapter"/> interface.
    ///     Adds the authenticated user's Auth0 access token to requests sent to the Integration API.
    /// </summary>
    public class HttpClientAdapter : IHttpClientAdapter
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpClientAdapter"/> class.
        /// </summary>
        /// <param name="httpClient">The underlying HTTP client used to make requests.</param>
        /// <param name="httpContextAccessor">Provides access to the current HTTP context and authenticated user.</param>
        /// <param name="configuration">Provides application configuration, including the Auth0 API audience.</param>
        /// <exception cref="ArgumentNullException">Thrown when a required dependency is null.</exception>
        public HttpClientAdapter(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        ///     Creates an HTTP request with the current user's Auth0 access token.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="requestUri">The relative URI of the API endpoint.</param>
        /// <param name="content">Optional HTTP content to include in the request.</param>
        /// <returns>An authenticated HTTP request message.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no HTTP context or access token is available.</exception>
        private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string requestUri, HttpContent? content = null)
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No active HTTP context is available.");

            var audience = _configuration["Auth0:Audience"]
                ?? throw new InvalidOperationException("The Auth0 API audience is not configured.");

            var accessToken = await httpContext.GetAccessTokenAsync(
                new AccessTokenRequest
                {
                    Audience = audience,
                    Scope = "messages:replay"
                });

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException(
                    "An API access token could not be obtained. Please sign in again.");
            }

            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = content
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            return request;
        }

        /// <summary>
        ///     Sends a GET request and deserializes the JSON response to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to.</typeparam>
        /// <param name="requestUri">The relative URI of the API endpoint.</param>
        /// <returns>The deserialized response, or null if the response body is empty.</returns>
        public async Task<T?> GetFromJsonAsync<T>(string requestUri)
        {
            using var request = await CreateRequestAsync(HttpMethod.Get, requestUri);
            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        /// <summary>
        ///     Sends a GET request and deserializes the JSON response to the specified type,
        ///     with cancellation support.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to.</typeparam>
        /// <param name="requestUri">The relative URI of the API endpoint.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the request.</param>
        /// <returns>The deserialized response, or null if the response body is empty.</returns>
        public async Task<T?> GetFromJsonAsync<T>(string requestUri, CancellationToken cancellationToken)
        {
            using var request = await CreateRequestAsync(HttpMethod.Get, requestUri);
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        }

        /// <summary>
        ///     Sends a POST request with the specified content.
        /// </summary>
        /// <param name="requestUri">The relative URI of the API endpoint.</param>
        /// <param name="content">The HTTP content to send, or null for an empty body.</param>
        /// <returns>The HTTP response message from the API.</returns>
        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent? content)
        {
            using var request = await CreateRequestAsync(HttpMethod.Post, requestUri, content);

            return await _httpClient.SendAsync(request);
        }

        /// <summary>
        ///     Sends a POST request with a JSON-serialized payload.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="requestUri">The relative URI of the API endpoint.</param>
        /// <param name="value">The value to serialize and send in the request body.</param>
        /// <returns>The HTTP response message from the API.</returns>
        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
        {
            using var content = JsonContent.Create(value);
            using var request = await CreateRequestAsync(HttpMethod.Post, requestUri, content);

            return await _httpClient.SendAsync(request);
        }
    }
}
