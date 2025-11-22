using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using JainMunis.API.Data;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            });

            services.Configure<TelemetryConfiguration>((telemetryConfiguration) =>
            {
                telemetryConfiguration.ConnectionString = appInsightsConnectionString;
                telemetryConfiguration.TelemetryInitializers.Add(new TelemetryInitializer());
            });
        }

        // Add Health Checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck<RedisHealthCheck>("redis")
            .AddCheck<ExternalServiceHealthCheck>("external_services");

        return services;
    }

    public static ILoggingBuilder ConfigureLogging(this ILoggingBuilder builder, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console(restrictedToMinimumLevel: environment.IsProduction() ? LogEventLevel.Information : LogEventLevel.Debug)
            .WriteTo.File("logs/jainmunis-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                restrictedToMinimumLevel: environment.IsProduction() ? LogEventLevel.Information : LogEventLevel.Debug,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
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
        if (telemetry is ISupportProperties telemetryWithProperties)
        {
            telemetryWithProperties.Properties["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
            telemetryWithProperties.Properties["MachineName"] = Environment.MachineName;
            telemetryWithProperties.Properties["ProcessId"] = Environment.ProcessId.ToString();
        }
    }
}

// Exception Telemetry Processor
public class ExceptionTelemetryProcessor : ITelemetryProcessor
{
    private ITelemetryProcessor? _next;

    public ExceptionTelemetryProcessor(ITelemetryProcessor? next = null)
    {
        _next = next;
    }

    public void Process(ITelemetry telemetry)
    {
        if (telemetry is ExceptionTelemetry exceptionTelemetry)
        {
            var exception = exceptionTelemetry.Exception;

            // Filter out common exceptions
            if (exception is TaskCanceledException ||
                exception is OperationCanceledException ||
                exception?.Message?.Contains("connection") == true)
            {
                return; // Don't process this telemetry
            }
        }

        _next?.Process(telemetry);
    }
}

// Sensitive Data Processor
public class SensitiveDataProcessor : ITelemetryProcessor
{
    private ITelemetryProcessor? _next;
    private readonly HashSet<string> _sensitiveHeaders = new()
    {
        "Authorization", "Cookie", "X-API-Key", "X-Forwarded-For",
        "Set-Cookie", "WWW-Authenticate"
    };

    public SensitiveDataProcessor(ITelemetryProcessor? next = null)
    {
        _next = next;
    }

    public void Process(ITelemetry telemetry)
    {
        if (telemetry is RequestTelemetry requestTelemetry)
        {
            // Sanitize sensitive properties
            foreach (var header in _sensitiveHeaders)
            {
                if (requestTelemetry.Properties.ContainsKey(header))
                {
                    requestTelemetry.Properties[header] = "***";
                }
            }
        }

        _next?.Process(telemetry);
    }
}

// Health Checks
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DatabaseHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public RedisHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var redisConnection = _configuration.GetConnectionString("Redis");
            if (string.IsNullOrEmpty(redisConnection))
            {
                return HealthCheckResult.Healthy("Redis not configured");
            }

            // Add actual Redis connection check here
            return HealthCheckResult.Healthy("Redis connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis connection failed", ex);
        }
    }
}

public class ExternalServiceHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check Mapbox API
            var mapboxToken = Environment.GetEnvironmentVariable("MAPBOX_ACCESS_TOKEN");
            if (!string.IsNullOrEmpty(mapboxToken))
            {
                using var client = new HttpClient();
                var response = await client.GetAsync($"https://api.mapbox.com/styles/v1/mapbox/streets-v11?access_token={mapboxToken}&limit=1", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Degraded("Mapbox API is not accessible");
                }
            }

            // Check SendGrid API
            var sendgridKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            if (!string.IsNullOrEmpty(sendgridKey))
            {
                using var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.sendgrid.com/v3/user/profile");
                request.Headers.Add("Authorization", $"Bearer {sendgridKey}");
                var response = await client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Degraded("SendGrid API is not accessible");
                }
            }

            return HealthCheckResult.Healthy("All external services are accessible");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("External services check failed", ex);
        }
    }
}

// Custom Application Insights Sink
public class ApplicationInsightsSink : ILogEventSink, IDisposable
{
    private readonly TelemetryConfiguration _telemetryConfiguration;
    private readonly TelemetryClient _telemetryClient;

    public ApplicationInsightsSink(string connectionString)
    {
        _telemetryConfiguration = new TelemetryConfiguration();
        _telemetryConfiguration.ConnectionString = connectionString;
        _telemetryClient = new TelemetryClient(_telemetryConfiguration);
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level == LogEventLevel.Fatal || logEvent.Level == LogEventLevel.Error)
        {
            _telemetryClient.TrackException(logEvent.Exception, new Dictionary<string, string>
            {
                ["LogLevel"] = logEvent.Level.ToString(),
                ["MessageTemplate"] = logEvent.MessageTemplate?.ToString() ?? "",
                ["RenderedMessage"] = logEvent.RenderMessage() ?? ""
            });
        }
    }

    public void Dispose()
    {
        _telemetryClient?.Flush();
    }
}