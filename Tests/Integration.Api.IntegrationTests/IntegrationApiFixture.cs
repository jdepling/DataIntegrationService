using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Integration.Api.IntegrationTests;

public sealed class IntegrationApiFixture : IDisposable
{
    private readonly string _connectionString;

    public HttpClient HttpClient { get; }

    public TimeSpan DeliveryTimeout { get; }

    public IntegrationApiFixture()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var apiBaseUrl = configuration["ApiBaseUrl"]
            ?? throw new InvalidOperationException("ApiBaseUrl is missing from appsettings.json.");

        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl)
        };

        DeliveryTimeout = TimeSpan.FromSeconds(configuration.GetValue<int>("DeliveryTimeoutSeconds", 30));

        var connectionString = configuration.GetConnectionString("SystemB")
            ?? throw new InvalidOperationException("ConnectionStrings:SystemB is missing from appsettings.json.");

        var password = Environment.GetEnvironmentVariable("SA_PASSWORD");

        if (string.IsNullOrWhiteSpace(password))
            password = ReadPasswordFromDotEnv();

        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            Password = password
        };

        _connectionString = builder.ConnectionString;
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

            command.Parameters.Add("@SourceId", SqlDbType.NVarChar, -1).Value = sourceId;

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

        throw new TimeoutException($"Message '{sourceId}' was not persisted in System B " +
            $"within {DeliveryTimeout.TotalSeconds} seconds.");
    }

    private static string ReadPasswordFromDotEnv()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(
                Path.Combine(directory.FullName, "DataIntegrationService.slnx")))
            {
                var envPath = Path.Combine(
                    directory.FullName,
                    "docker",
                    "test-environments",
                    "api-integration",
                    ".env");

                if (!File.Exists(envPath))
                {
                    throw new FileNotFoundException(
                        "The API integration test .env file was not found. " +
                        "Create it from .env.example, or set SA_PASSWORD.",
                        envPath);
                }

                foreach (var line in File.ReadLines(envPath))
                {
                    var trimmed = line.Trim();

                    if (!trimmed.StartsWith("SA_PASSWORD=",StringComparison.Ordinal))
                        continue;

                    var password = trimmed["SA_PASSWORD=".Length..].Trim();

                    if (!string.IsNullOrWhiteSpace(password))
                        return password;
                }

                throw new InvalidOperationException($"SA_PASSWORD is missing or empty in '{envPath}'.");
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the DataIntegrationService.slnx repository root.");
    }

    public void Dispose()
    {
        HttpClient.Dispose();
    }

    public sealed record SavedMessage(
        int CustomerId,
        string Name,
        decimal Amount,
        string Status,
        DateTime CreatedAt);
}
