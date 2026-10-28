using CCTV.Api.Extensions;
using CCTV.Api.Providers;
using CCTV.Application;
using CCTV.Infrastructure;
using CCTV.Infrastructure.SignalR;
using CCTV.Persistence;
using CCTV.Persistence.Contexts;
using CCTV.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Web Tenant & User Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();
builder.Services.AddScoped<ICurrentUserContext, HttpContextCurrentUserContext>();

// 2. Controllers & Routing
builder.Services.AddControllers();

// 3. CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// 4. Authentication (Pure from appsettings.json) & Swagger
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

// 5. Application, Persistence & Infrastructure Layers
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// 6. Apply Database Migrations & Conditional Seeding on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CCTVAnalyticsDbContext>();
    var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    await dbContext.Database.MigrateAsync();

    bool runSeeder = args.Contains("--dbseeder", StringComparer.OrdinalIgnoreCase)
                     || configuration.GetValue<bool>("Database:RunSeeder", false);
    if (runSeeder)
    {
        await CCTV.Persistence.Seed.DbSeeder.SeedAsync(dbContext, tenantProvider);
    }
}

// 7. HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));
app.MapControllers();

// 8. SignalR Hub
app.MapHub<EngineTelemetryHub>("/hubs/telemetry");

app.Run();
