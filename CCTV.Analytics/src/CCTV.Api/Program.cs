using CCTV.Api.Providers;
using CCTV.Application;
using CCTV.Infrastructure;
using CCTV.Infrastructure.SignalR;
using CCTV.Persistence;
using CCTV.Persistence.Contexts;
using CCTV.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Web Tenant Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();
builder.Services.AddScoped<ICurrentUserContext, HttpContextCurrentUserContext>();

// Add Controllers
builder.Services.AddControllers();

// Add CORS
var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply Database Migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CCTVAnalyticsDbContext>();
    var tenantProvider = scope.ServiceProvider.GetRequiredService<CCTV.Shared.Interfaces.ITenantProvider>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    await dbContext.Database.MigrateAsync();

    // Seeder only executes if explicitly requested via CLI argument `--dbseeder` or appsettings config
    bool runSeeder = args.Contains("--dbseeder", StringComparer.OrdinalIgnoreCase) 
                     || configuration.GetValue<bool>("Database:RunSeeder", false);
    if (runSeeder)
    {
        await CCTV.Persistence.Seed.DbSeeder.SeedAsync(dbContext, tenantProvider);
    }
}

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("Frontend");

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.MapControllers();

// Map SignalR Telemetry Hub Endpoint
app.MapHub<EngineTelemetryHub>("/hubs/telemetry");

app.Run();
