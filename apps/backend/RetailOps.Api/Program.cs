using RetailOps.Api.Middleware;
using RetailOps.Identity.Infrastructure;
using RetailOps.Infrastructure;
using RetailOps.Infrastructure.Legacy;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Platform.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddLegacyInfrastructure(builder.Configuration);
builder.Services.AddStoreSettingsModule();
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddPlatformModule();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await LegacyDatabaseBootstrap.EnsureDevSchemaAsync(app.Services);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "RetailOps.Api" }));

app.Run();

public partial class Program { }
