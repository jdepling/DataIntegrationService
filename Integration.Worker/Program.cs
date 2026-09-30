using Integration.Data;
using Integration.Worker.Services;
using Microsoft.EntityFrameworkCore;

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
            });

            builder.Services.AddScoped<IFailedMessageService, FailedMessageService>();
            builder.Services.AddScoped<IMessageProcessor, MessageProcessor>();

            var host = builder.Build();
            host.Run();
        }
    }
}