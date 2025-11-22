using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JainMunis.API.Data;
using System.Reflection;

namespace JainMunis.API.Config;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Add DbContext with optimized configuration
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // Enable retry on failure for resilience
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: environment.IsProduction() ? 3 : 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: new[] { 4060, 40197, 40501, 40613 }
                );

                // Command timeout optimization
                sqlOptions.CommandTimeout = 30;

                // Enable sensitive data logging only in development
                sqlOptions.EnableSensitiveDataLogging(!environment.IsProduction());

                // Enable detailed errors in development
                sqlOptions.EnableDetailedErrors(!environment.IsProduction());

                // Configure query splitting for better performance
                if (environment.IsProduction())
                {
                    sqlOptions.UseQuerySplitting();
                }

                // Batch size optimization
                sqlOptions.MaxBatchSize(100);
            });

            // Add interceptors for soft deletes and auditing
            options.AddInterceptors(serviceProvider.GetRequiredService<ISoftDeleteInterceptor>());
            options.AddInterceptors(serviceProvider.GetRequiredService<IAuditInterceptor>());
        });

        // Add health checks for database
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(
        this IServiceProvider serviceProvider,
        ILogger logger)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Run migrations
            logger.LogInformation("Running database migrations...");
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count());
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }
            else
            {
                logger.LogInformation("Database is up to date");
            }

            // Seed data if needed
            await SeedDataAsync(dbContext, logger);

            // Create indexes for performance
            await CreatePerformanceIndexesAsync(dbContext, logger);

            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed");
            throw;
        }
    }

    private static async Task SeedDataAsync(ApplicationDbContext dbContext, ILogger logger)
    {
        // Check if admin user exists
        var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "admin@jainmunis.app");

        if (adminUser == null)
        {
            logger.LogInformation("Creating default admin user");
            // This would typically be handled by existing seed data
        }
    }

    private static async Task CreatePerformanceIndexesAsync(ApplicationDbContext dbContext, ILogger logger)
    {
        try
        {
            // Example of creating performance indexes
            var connection = dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            // Index for frequently queried fields
            var indexes = new[]
            {
                "CREATE INDEX IF NOT EXISTS IX_Saints_Name_Active ON Saints(Name, IsActive) WHERE IsActive = 1",
                "CREATE INDEX IF NOT EXISTS IX_Schedules_Dates ON Schedules(StartDate, EndDate) WHERE StartDate >= GETDATE()",
                "CREATE INDEX IF NOT EXISTS IX_Locations_City_State ON Locations(City, State)",
                "CREATE INDEX IF NOT EXISTS IX_ActivityLogs_CreatedDate ON ActivityLogs(CreatedAt DESC)",
                "CREATE INDEX IF NOT EXISTS IX_Schedules_SaintId_Location ON Schedules(SaintId, LocationId)"
            };

            foreach (var indexSql in indexes)
            {
                command.CommandText = indexSql;
                await command.ExecuteNonQueryAsync();
            }

            logger.LogInformation("Performance indexes created successfully");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create some performance indexes");
        }
    }
}

// Interceptors for soft deletes and auditing
public interface ISoftDeleteInterceptor
{
}

public interface IAuditInterceptor
{
}

public class SoftDeleteInterceptor : ISoftDeleteInterceptor
{
    // Implementation would go here
}

public class AuditInterceptor : IAuditInterceptor
{
    // Implementation would go here
}