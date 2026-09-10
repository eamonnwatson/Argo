using Argo.Application.Repositories;
using Argo.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Argo.Data;

/// <summary>
/// Provides startup extension methods that register the Argo SQLite persistence
/// layer (EF Core <see cref="ArgoDbContext"/> and repositories) and apply pending
/// migrations at application boot.
/// </summary>
/// <remarks>
/// The database schema is managed with real EF Core migrations (see
/// <c>src/Argo.Persistence/Migrations</c>). Schema changes must be introduced via
/// <c>dotnet ef migrations add &lt;Name&gt; --project src/Argo.Persistence/Argo.Persistence.csproj --startup-project src/Argo.Persistence/Argo.Persistence.csproj --output-dir Migrations</c>
/// and applied at startup by <see cref="InitializeArgoDatabase"/> calling <c>Database.Migrate()</c>.
/// Do not reintroduce <c>Database.EnsureCreated()</c>, as it is incompatible with migrations.
/// </remarks>
public static class PersistenceServiceExtensions
{
    /// <summary>
    /// Registers the <see cref="ArgoDbContext"/> (backed by SQLite) and the Argo
    /// persistence repositories with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register persistence services with.</param>
    /// <param name="sqliteConnectionString">The SQLite connection string (e.g. <c>Data Source=argo.db</c>) to use for the database context.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance to allow fluent configuration.</returns>
    public static IServiceCollection AddArgoPersistence(this IServiceCollection services, string sqliteConnectionString)
    {
        services.AddDbContext<ArgoDbContext>(options => options.UseSqlite(sqliteConnectionString));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IWorkItemRepository, WorkItemRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IRaidItemRepository, RaidItemRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        return services;
    }

    /// <summary>
    /// Applies any pending EF Core migrations to the application's SQLite database and configures
    /// it for write-ahead logging.
    /// </summary>
    /// <param name="services">The root service provider used to resolve the <see cref="ArgoDbContext"/>.</param>
    /// <remarks>
    /// The database is migrated during startup so deployments do not require a separate
    /// manual database bootstrap step. Schema changes must be made via new EF Core migrations
    /// (<c>dotnet ef migrations add &lt;Name&gt;</c>) rather than by relying on EnsureCreated().
    /// </remarks>
    public static void InitializeArgoDatabase(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ArgoDbContext>();
        db.Database.Migrate();
        db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    }
}
