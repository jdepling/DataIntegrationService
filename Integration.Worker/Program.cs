using Integration.Worker.Services;

namespace Integration.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.Configure<RabbitMqOptions>(
            builder.Configuration.GetSection("RabbitMq"));
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddHttpClient<ISystemBClient, SystemBClient>((serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                client.BaseAddress = new Uri(
                    configuration["SystemB:BaseUrl"]!);
            });

            var host = builder.Build();
            host.Run();
        }
    }
}