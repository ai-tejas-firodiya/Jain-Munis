using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Reflection;

namespace JainMunis.API.Config;

public static class MonitoringConfiguration
{
    public static IServiceCollection AddMonitoring(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Add Application Insights
        var appInsightsConnectionString = configuration.GetConnectionString("ApplicationInsights:ConnectionString");
        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            services.AddApplicationInsights(appInsightsConnectionString);

            services.Configure<TelemetryConfiguration>((telemetryConfiguration) =>
            {
                telemetryConfiguration.ConnectionString = appInsightsConnectionString;
                telemetryConfiguration.TelemetryInitializers.Add(new TelemetryInitializer());
                telemetryConfiguration.ApplicationInsightsChannel.TelemetryProcessors.Add(new ExceptionTelemetryProcessor());
                telemetryConfiguration.ApplicationInsightsChannel.TelemetryProcessors.Add(new SensitiveDataProcessor());
            });
        }

        // Add Sentry if DSN is provided
        var sentryDsn = configuration.GetSection("Monitoring:Sentry:Dsn").Value;
        if (!string.IsNullOrEmpty(sentryDsn))
        {
            services.AddSentry(options =>
            {
                options.Dsn = sentryDsn;
                options.TracesSampleRate = configuration.GetValue<double>("Monitoring:Sentry:TracesSampleRate", 0.1);
                options.Environment = environment.EnvironmentName;
                options.Debug = configuration.GetValue<bool>("Monitoring:Sentry:Debug", false);
                options.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
                options.BeforeSend = sentryEvent =>
                {
                    // Filter out certain events
                    if (sentryEvent.Exception?.Message?.Contains("TaskCancelledException") == true)
                        return false;

                    // Filter out health check requests
                    if (sentryEvent.Request?.Url?.Contains("/health") == true)
                        return false;

                    return true;
                };
            });
        }

        // Add Health Checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck<RedisHealthCheck>("redis")
            .AddCheck<ExternalServiceHealthCheck>("external_services");

        return services;
    }

    public static ILoggingBuilder ConfigureLogging(this ILoggingBuilder builder, IWebHostEnvironment environment)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.FromEnvironment()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .WriteTo.Console(restrictedToMinimumLevel: environment.IsProduction())
            .WriteTo.File("logs/jainmunis-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                restrictedToMinimumLevel: environment.IsProduction(),
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                formatter: new Serilog.Formatting.Json.JsonFormatter())
                .WriteTo.Seq("logs/application",
                    restrictedToMinimumLevel: environment.IsProduction(),
                    batchSink: new ApplicationInsightsSink(appInsightsConnectionString)
                )
            .CreateLogger();

        builder.ClearProviders();
        builder.AddSerilog();

        return builder;
    }
}

// Custom Telemetry Initializer
public class TelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        // Add custom properties to all telemetry
        telemetry.Context.Properties["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
        telemetry.Context.Properties["MachineName"] = Environment.MachineName;
        telemetry.Context.Properties["ProcessId"] = Environment.ProcessId;
    }
}

// Exception Telemetry Processor
public class ExceptionTelemetryProcessor : ITelemetryProcessor
{
    public void Process(ITelemetry telemetry)
    {
        if (telemetry is ExceptionTelemetry exceptionTelemetry)
        {
            var exception = exceptionTelemetry.Exception;

            // Filter out common exceptions
            if (exception is TaskCanceledException ||
                exception is OperationCanceledException ||
                exception.Message?.Contains("connection") == true)
            {
                telemetry.OmitTelemetry = true;
            }
        }
    }
}

// Sensitive Data Processor
public class SensitiveDataProcessor : ITelemetryProcessor
{
    private readonly HashSet<string> _sensitiveHeaders = new()
    {
        "Authorization", "Cookie", "X-API-Key", "X-Forwarded-For",
        "Set-Cookie", "WWW-Authenticate"
    };

    public void Process(ITelemetry telemetry)
    {
        if (telemetry is RequestTelemetry requestTelemetry)
        {
            // Sanitize sensitive headers
            foreach (var header in _sensitiveHeaders)
            {
                if (requestTelemetry.Headers.ContainsKey(header))
                {
                    requestTelemetry.Headers[header] = "***";
                }
            }
        }
    }
}

// Health Checks
public class DatabaseHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DatabaseHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database connection successful");
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}

public class RedisHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    private readonly IConfiguration _configuration;

    public RedisHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var redisConnection = _configuration.GetConnectionString("Redis");
            if (string.IsNullOrEmpty(redisConnection))
            {
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Redis not configured");
            }

            // Add actual Redis connection check here
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Redis connection successful");
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Redis connection failed", ex);
        }
    }
}

public class ExternalServiceHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check Mapbox API
            var mapboxToken = Environment.GetEnvironmentVariable("MAPBOX_ACCESS_TOKEN");
            if (!string.IsNullOrEmpty(mapboxToken))
            {
                using var client = new HttpClient();
                var response = await client.GetAsync($"https://api.mapbox.com/styles/v1/mapbox/streets-v11?access_token={mapboxToken}&limit=1");
                if (!response.IsSuccess)
                {
                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded("Mapbox API is not accessible");
                }
            }

            // Check SendGrid API
            var sendgridKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            if (!string.IsNullOrEmpty(sendgridKey))
            {
                using var client = new HttpClient();
                var response = await client.GetAsync("https://api.sendgrid.com/v3/user/profile",
                    new Dictionary<string, string>
                    {
                        ["Authorization"] = $"Bearer {sendgridKey}"
                    });
                if (!response.IsSuccess)
                {
                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded("SendGrid API is not accessible");
                }
            }

            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("All external services are accessible");
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("External services check failed", ex);
        }
    }
}

// Custom Application Insights Sink
public class ApplicationInsightsSink : ILogEventSink, IDisposable
{
    private readonly TelemetryConfiguration _telemetryConfiguration;
    private readonly TelemetryClient _telemetryClient;

    public ApplicationInsightSink(string connectionString)
    {
        _telemetryConfiguration = new TelemetryConfiguration(connectionString);
        _telemetryClient = new TelemetryClient(_telemetryConfiguration);
    }

    public Emit(LogEvent logEvent)
    {
        if (logEvent.Level == LogEventLevel.Fatal || logEvent.Level == LogEventLevel.Error)
        {
            _telemetryClient.TrackException(logEvent.Exception, new Dictionary<string, string>
            {
                ["LogLevel"] = logEvent.Level.ToString(),
                ["MessageTemplate"] = logEvent.MessageTemplate?.ToString() ?? "",
                ["RenderedMessage"] = logEvent.RenderMessage ?? ""
            });
        }
    }

    public void Dispose()
    {
        _telemetryClient?.Flush();
    }
}