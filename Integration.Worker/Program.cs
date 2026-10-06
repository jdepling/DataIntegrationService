using Integration.Data;
using Integration.Worker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;

namespace Integration.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.Configure<RabbitMqOptions>(
            builder.Configuration.GetSection("RabbitMq"));

            builder.Services.AddPooledDbContextFactory<IntegrationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("IntegrationDatabase")));

            builder.Services.AddHostedService<Worker>();
            builder.Services.AddHttpClient<ISystemBClient, SystemBClient>((serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                client.BaseAddress = new Uri(
                    configuration["SystemB:BaseUrl"]!);
            }).AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(2);

                options.Retry.OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Polly retry #{args.AttemptNumber + 1} " +
                        $"for {args.Outcome.Result?.StatusCode}");

                    return default;
                };
            });

            builder.Services.AddScoped<IFailedMessageService, FailedMessageService>();
            builder.Services.AddScoped<IMessageProcessor, MessageProcessor>();

            var host = builder.Build();
            host.Run();
        }
    }
}