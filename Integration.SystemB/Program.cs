using Integration.SystemB.Data;
using Integration.SystemB.Services;
using Microsoft.EntityFrameworkCore;

namespace Integration.SystemB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddDbContext<SystemBDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("SystemBDatabase")));

            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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