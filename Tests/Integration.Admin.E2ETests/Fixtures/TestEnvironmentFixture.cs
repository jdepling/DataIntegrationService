using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Integration.Admin.E2ETests.Fixtures;

public sealed class TestEnvironmentFixture : IAsyncLifetime
{
    private ComposeContainer? _environment;

    public async Task InitializeAsync()
    {
        var repositoryRoot = FindRepositoryRoot();

        var envFile = Path.Combine(
            repositoryRoot,
            "docker",
            "test-environments",
            "admin-e2e",
            ".env");

        LoadEnvironmentFile(envFile);

        var composeFile = Path.Combine(
            repositoryRoot,
            "docker",
            "test-environments",
            "admin-e2e",
            "compose.yml");

        _environment = new ComposeBuilder("docker:29.8.1-cli")
            .WithComposeFile(composeFile)
            .WithCopyFilesInContainer(
                ".env",
                "../../../Integration.Api",
                "../../../Integration.Data",
                "../../../Integration.OutboxPublisher",
                "../../../Integration.Worker",
                "../../../Integration.SystemB",
                "../../../Integration.Admin",
                "../../../rabbitmq",
                "../../../DataIntegrationService.slnx")
            .WithPull(false)
            .WithComposeUpOption("--build")
            .WithProjectNamePrefix("data-integration-admin-e2e")
            .Build();

        Console.WriteLine("Starting E2E environment...");

        await _environment.StartAsync();

        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        var deadline = DateTime.UtcNow.AddMinutes(2);
        var seleniumReady = false;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var response = await httpClient.GetAsync(
                    "http://localhost:4445/status");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Selenium is ready.");
                    seleniumReady = true;
                    break;
                }
            }
            catch (HttpRequestException)
            {
                // Selenium may not be ready yet.
            }
            catch (TaskCanceledException)
            {
                // This request timed out; try again.
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        if (!seleniumReady)
        {
            throw new TimeoutException(
                "Selenium did not become ready within two minutes.");
        }

    }


    public async Task DisposeAsync()
    {
        if (_environment is not null)
        {
            await _environment.DisposeAsync();
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(
                    directory.FullName,
                    "DataIntegrationService.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the DataIntegrationService solution root.");
    }

    private static void LoadEnvironmentFile(string path)
    {
        foreach (var line in File.ReadLines(path))
        {
            var separatorIndex = line.IndexOf('=');

            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (string.IsNullOrWhiteSpace(
                    Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }


}