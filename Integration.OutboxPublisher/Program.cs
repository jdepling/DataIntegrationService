using Integration.Data;
using Integration.OutboxPublisher.Services;
using Microsoft.EntityFrameworkCore;

namespace Integration.OutboxPublisher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.Configure<RabbitMqOptions>(
            builder.Configuration.GetSection("RabbitMq"));

            builder.Services.AddDbContextFactory<IntegrationDbContext>(options =>
                options.UseSqlServer(
            builder.Configuration.GetConnectionString("IntegrationDatabase")));

            builder.Services.AddHostedService<Worker>();
            builder.Services.AddTransient<IPublishService, PublishService>();
            builder.Services.AddTransient<IOutboxService, OutboxService>();

            var host = builder.Build();
            host.Run();
        }
    }
}