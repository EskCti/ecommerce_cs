using System.Text.Json.Serialization;
using RetailOps.Api.Middleware;
using RetailOps.Identity.Infrastructure;
using RetailOps.Infrastructure;
using RetailOps.Infrastructure.Legacy;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Platform.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var messages = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(error => error.ErrorMessage))
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToList();

            var error = messages.Count > 0
                ? string.Join(" • ", messages)
                : "Dados inválidos.";

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { error });
        };
    });
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
builder.Services.AddCrmModule();
builder.Services.AddCatalogModule();
builder.Services.AddSalesModule();
builder.Services.AddFinanceModule();
builder.Services.AddReturnsModule();
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddPlatformModule();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
    await LegacyDatabaseBootstrap.EnsureDevSchemaAsync(app.Services);

if (app.Environment.IsDevelopment())
{
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
