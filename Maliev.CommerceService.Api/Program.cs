using Maliev.Aspire.ServiceDefaults;
using Maliev.CommerceService.Application.Interfaces;
using Maliev.CommerceService.Application.Services;
using Maliev.CommerceService.Infrastructure.Persistence;
using Maliev.CommerceService.Infrastructure.Repositories;

using ApplicationCommerceService = Maliev.CommerceService.Application.Services.CommerceService;

using ILoggerFactory loggerFactory = LoggerFactory.Create(logBuilder => logBuilder.AddConsole());
ILogger bootstrapLogger = loggerFactory.CreateLogger("Program");

try
{
    Program.Log.StartingHost(bootstrapLogger, "Commerce Service");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.AddGoogleSecretManagerVolume();
    builder.AddServiceDefaults();
    builder.AddStandardMiddleware(options => options.EnableRequestLogging = true);
    builder.AddServiceMeters("commerce-meter");
    builder.AddStandardCache("commerce:");
    builder.AddMassTransitWithRabbitMq();
    builder.AddPostgresDbContext<CommerceDbContext>(connectionName: "CommerceDbContext");
    builder.AddStandardCors();
    builder.AddDefaultApiVersioning();
    builder.AddJwtAuthentication();
    builder.Services.AddPermissionAuthorization();

    builder.AddStandardOpenApi(
        title: "MALIEV Commerce Service API",
        description: "Storefront catalog, cart, checkout, and shop order service. This service references CustomerService customers and stays separate from manufacturing projects.");

    builder.Services.AddScoped<ICommerceRepository, CommerceRepository>();
    builder.Services.AddScoped<ICommerceService, ApplicationCommerceService>();
    builder.Services.AddControllers();
    builder.AddStandardRateLimiting();
    builder.AddIAMServiceClient("commerce");

    WebApplication app = builder.Build();
    ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();

    await app.MigrateDatabaseAsync<CommerceDbContext>();
    await CommerceCatalogSeeder.SeedStarterCatalogAsync(app.Services, app.Lifetime.ApplicationStopping);

    app.UseStandardMiddleware();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseRouting();
    app.UseCors();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapDefaultEndpoints(servicePrefix: "commerce");
    app.MapApiDocumentation(servicePrefix: "commerce");

    Program.Log.ServiceStarted(logger, "Commerce Service");
    await app.RunAsync();
}
catch (Exception ex)
{
    Program.Log.HostTerminated(bootstrapLogger, ex, "Commerce Service");
    Console.Out.Flush();
    Console.Error.Flush();
    throw;
}
finally
{
    loggerFactory.Dispose();
}

/// <summary>
/// Main program class for Commerce Service.
/// </summary>
public partial class Program
{
    internal static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Starting {ServiceName} host")]
        public static partial void StartingHost(ILogger logger, string serviceName);

        [LoggerMessage(Level = LogLevel.Critical, Message = "{ServiceName} host terminated unexpectedly during startup")]
        public static partial void HostTerminated(ILogger logger, Exception ex, string serviceName);

        [LoggerMessage(Level = LogLevel.Information, Message = "{ServiceName} started successfully")]
        public static partial void ServiceStarted(ILogger logger, string serviceName);
    }
}
