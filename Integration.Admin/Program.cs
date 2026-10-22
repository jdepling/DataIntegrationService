using Auth0.AspNetCore.Authentication;
using Integration.Admin.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim(
                  "https://data-integration-service/roles",
                  "Integration Admin"));
});


builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"]!;
    options.ClientId = builder.Configuration["Auth0:ClientId"]!;
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"]!;
    options.Scope = "openid profile email";
})
.WithAccessToken(options =>
{
    options.Audience = builder.Configuration["Auth0:Audience"];
    options.ScopeByAudience = new Dictionary<string, string>
    {
        ["https://data-integration-service-api"] = "messages:replay"
    };
});


builder.Services.AddHttpClient<HttpClientAdapter>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IHttpClientAdapter>(sp => sp.GetRequiredService<HttpClientAdapter>());
builder.Services.AddScoped<IAdminApiClient, AdminApiClient>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=FailedMessages}/{action=Index}/{id?}");

app.Run();
