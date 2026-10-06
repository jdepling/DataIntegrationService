using Integration.Api.Services;
using Integration.Data;
using Microsoft.EntityFrameworkCore;

namespace Integration.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<IntegrationDbContext>(options =>
                options.UseSqlServer(
                builder.Configuration.GetConnectionString("IntegrationDatabase"),
                 sqlOptions =>
                 {
                     sqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
                 }));
            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddScoped<IReplayMessageService, ReplayMessageService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
