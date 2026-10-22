
using System.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Integration.Api.IntegrationTests;

public sealed class IntegrationApiFixture : IAsyncLifetime
{
    private readonly string _connectionString;
    private readonly string _auth0Domain;
    private readonly string _auth0Audience;
    private readonly string _auth0ClientId;
    private readonly string _auth0ClientSecret;

    public HttpClient HttpClient { get; }
    public HttpClient UnauthenticatedHttpClient { get; }

    public TimeSpan DeliveryTimeout { get; }

    public IntegrationApiFixture()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var apiBaseUrl = configuration["ApiBaseUrl"]
            ?? throw new InvalidOperationException(
                "ApiBaseUrl is missing from appsettings.json.");

        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl)
        };

        UnauthenticatedHttpClient = new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl)
        };

        DeliveryTimeout = TimeSpan.FromSeconds(
            configuration.GetValue<int>("DeliveryTimeoutSeconds", 30));

        _auth0Domain = GetRequiredSetting("AUTH0_DOMAIN");
        _auth0Audience = GetRequiredSetting("AUTH0_AUDIENCE");
        _auth0ClientId = GetRequiredSetting("AUTH0_TEST_CLIENT_ID");
        _auth0ClientSecret = GetRequiredSetting("AUTH0_TEST_CLIENT_SECRET");

        var connectionString = configuration.GetConnectionString("SystemB")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:SystemB is missing from appsettings.json.");

        var password = GetRequiredSetting("SA_PASSWORD");

        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            Password = password
        };

        _connectionString = builder.ConnectionString;
    }

    public async Task InitializeAsync()
    {
        using var tokenClient = new HttpClient();

        var tokenResponse = await tokenClient.PostAsJsonAsync(
            $"https://{_auth0Domain.TrimEnd('/')}/oauth/token",
            new
            {
                client_id = _auth0ClientId,
                client_secret = _auth0ClientSecret,
                audience = _auth0Audience,
                grant_type = "client_credentials"
            });

        tokenResponse.EnsureSuccessStatusCode();

        var token = await tokenResponse.Content
            .ReadFromJsonAsync<Auth0TokenResponse>();

        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            throw new InvalidOperationException(
                "Auth0 did not return an access token.");
        }

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);
    }

    public async Task<SavedMessage?> WaitForMessageAsync(string sourceId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var deadline = DateTime.UtcNow + DeliveryTimeout;

        while (DateTime.UtcNow < deadline)
        {
            using var command = connection.CreateCommand();

            command.CommandText = """
                SELECT CustomerId, Name, Amount, Status, CreatedAt
                FROM dbo.Messages
                WHERE SourceId = @SourceId;
                """;

            command.Parameters.Add("@SourceId", SqlDbType.NVarChar, -1)
                .Value = sourceId;

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new SavedMessage(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetDecimal(2),
                    reader.GetString(3),
                    reader.GetDateTime(4));
            }

            await reader.CloseAsync();
            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        throw new TimeoutException(
            $"Message '{sourceId}' was not persisted in System B " +
            $"within {DeliveryTimeout.TotalSeconds} seconds.");
    }

    private static string GetRequiredSetting(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (!string.IsNullOrWhiteSpace(value))
            return value;

        var envPath = FindEnvFile();

        foreach (var line in File.ReadLines(envPath))
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith($"{name}=", StringComparison.Ordinal))
            {
                value = trimmed[(name.Length + 1)..].Trim();

                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        throw new InvalidOperationException(
            $"{name} is missing or empty. Set it as an environment " +
            $"variable or add it to '{envPath}'.");
    }

    private static string FindEnvFile()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(
                directory.FullName, "DataIntegrationService.slnx")))
            {
                return Path.Combine(
                    directory.FullName,
                    "docker",
                    "test-environments",
                    "api-integration",
                    ".env");
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the DataIntegrationService.slnx repository root.");
    }

    public Task DisposeAsync()
    {
        HttpClient.Dispose();
        UnauthenticatedHttpClient.Dispose();

        return Task.CompletedTask;
    }

    private sealed class Auth0TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }

    public sealed record SavedMessage(
        int CustomerId,
        string Name,
        decimal Amount,
        string Status,
        DateTime CreatedAt);
}
