namespace Integration.Admin.Services
{
    /// <summary>
    ///     Provides an adapter for HTTP client operations, implementing the <see cref="IHttpClientAdapter"/> interface.
    ///     Encapsulates HTTP communication methods for GET and POST operations with JSON serialization support.
    /// </summary>
    public class HttpClientAdapter : IHttpClientAdapter
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpClientAdapter"/> class.
        /// </summary>
        /// <param name="httpClient">
        ///     The underlying HTTP client to use for making requests.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="httpClient"/> is null.
        /// </exception>
        public HttpClientAdapter(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        ///     Sends a GET request and deserializes the JSON response to the specified type.
        /// </summary>
        /// <typeparam name="T">
        ///     The type to deserialize the response to.
        /// </typeparam>
        /// <param name="requestUri">
        ///     The URI of the request.
        /// </param>
        /// <returns>
        ///     The deserialized response of type <typeparamref name="T"/>, or null if the response cannot be deserialized.
        /// </returns>
        public async Task<T?> GetFromJsonAsync<T>(string requestUri)
        {
            return await _httpClient.GetFromJsonAsync<T>(requestUri);
        }

        /// <summary>
        ///     Sends a GET request and deserializes the JSON response to the specified type with cancellation support.
        /// </summary>
        /// <typeparam name="T">
        ///     The type to deserialize the response to.
        /// </typeparam>
        /// <param name="requestUri">
        ///     The URI of the request.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to cancel the request.
        /// </param>
        /// <returns>
        ///     The deserialized response of type <typeparamref name="T"/>, or null if the response cannot be deserialized.
        /// </returns>
        public async Task<T?> GetFromJsonAsync<T>(string requestUri, CancellationToken cancellationToken)
        {
            return await _httpClient.GetFromJsonAsync<T>(requestUri, cancellationToken);
        }

        /// <summary>
        ///     Sends a POST request with the specified content.
        /// </summary>
        /// <param name="requestUri">
        ///     The URI of the request.
        /// </param>
        /// <param name="content">
        ///     The HTTP content to send in the request body, or null for an empty body.
        /// </param>
        /// <returns>
        ///     The HTTP response message from the server.
        /// </returns>
        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent? content)
        {
            return await _httpClient.PostAsync(requestUri, content);
        }

        /// <summary>
        ///     Sends a POST request with a JSON-serialized payload.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the value to serialize and send.
        /// </typeparam>
        /// <param name="requestUri">
        ///     The URI of the request.
        /// </param>
        /// <param name="value">
        ///     The value to serialize as JSON and send in the request body.
        /// </param>
        /// <returns>
        ///     The HTTP response message from the server.
        /// </returns>
        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
        {
            return await _httpClient.PostAsJsonAsync(requestUri, value);
        }
    }
}
