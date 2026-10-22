using Integration.Api.Services;
using Integration.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

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

            builder.Services.AddSwaggerGen(options =>
            {
                var authority = builder.Configuration["Auth0:Authority"]!
                    .TrimEnd('/');

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Description = "Authenticate with Auth0",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{authority}/authorize"),
                            TokenUrl = new Uri($"{authority}/oauth/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                ["messages:submit"] = "Submit integration messages",
                                ["messages:replay"] = "Replay failed messages"
                            }
                        }
                    }
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        new[] { "messages:submit", "messages:replay" }
                    }
                });
            });

            builder.Services.AddDbContext<IntegrationDbContext>(options =>
                options.UseSqlServer(
                builder.Configuration.GetConnectionString("IntegrationDatabase"),
                 sqlOptions =>
                 {
                     sqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
                 }));
            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddScoped<IReplayMessageService, ReplayMessageService>();

            // Auth0 JWT bearer authentication.
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = builder.Configuration["Auth0:Authority"];
                    options.Audience = builder.Configuration["Auth0:Audience"];
                });

            // API permissions.
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("SubmitMessages", policy =>
                    policy.RequireAuthenticatedUser()
                          .RequireClaim("permissions", "messages:submit"));

                options.AddPolicy("ReplayMessages", policy =>
                    policy.RequireAuthenticatedUser()
                          .RequireClaim("permissions", "messages:replay"));
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();


                app.UseSwaggerUI(options =>
                {
                    options.OAuthClientId(
                        builder.Configuration["Auth0:SwaggerClientId"]);

                    options.OAuthUsePkce();

                    options.OAuthAdditionalQueryStringParams(
                        new Dictionary<string, string>
                        {
                            ["audience"] = builder.Configuration["Auth0:Audience"]!
                        });
                });

            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
