using CCTV.Api.Providers;
using CCTV.Application;
using CCTV.Infrastructure;
using CCTV.Persistence;
using CCTV.Persistence.Contexts;
using CCTV.Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add Web Tenant Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();
builder.Services.AddScoped<ICurrentUserContext, HttpContextCurrentUserContext>();

// Add Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure Database & Tables created on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CCTVAnalyticsDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();
