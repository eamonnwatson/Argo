using Argo.Application.Repositories;
using Argo.Data.Repositories;
using Argo.Domain.Entities;
using Argo.Domain.Enums;
using Argo.Domain.ValueObjects;
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

#if DEBUG
        SeedDebugData(db);
#endif
    }

#if DEBUG
    private static void SeedDebugData(ArgoDbContext db)
    {
        if (db.Projects.Any() || db.WorkItems.Any() || db.Activities.Any() || db.RaidItems.Any() || db.Users.Any())
            return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var alexId = UserId.Create("alex.chen").Value;
        var jamieId = UserId.Create("jamie.owens").Value;
        var priyaId = UserId.Create("priya.patel").Value;
        var mateoId = UserId.Create("mateo.garcia").Value;
        var eamonnId = UserId.Create("eamonn.watson").Value;

        db.Users.AddRange(
            User.Create(alexId, "Alex Chen", true, "alex.chen@argo.local").Value,
            User.Create(jamieId, "Jamie Owens", true, "jamie.owens@argo.local").Value,
            User.Create(priyaId, "Priya Patel", false, "priya.patel@argo.local").Value,
            User.Create(mateoId, "Mateo Garcia", false, "mateo.garcia@argo.local").Value,
            User.Create(eamonnId, "Eamonn Watson", true, "eamonn.watson@argo.local").Value);

        var atlasProjectId = ProjectId.Create("proj-atlas-modernization").Value;
        var pulseProjectId = ProjectId.Create("proj-pulse-analytics").Value;
        var orbitProjectId = ProjectId.Create("proj-orbit-onboarding").Value;

        db.Projects.AddRange(
            Project.Create(
                atlasProjectId,
                "Atlas Platform Modernization",
                alexId,
                ProjectStatus.InProgress,
                ProjectHealth.OnTrack,
                ProjectPriority.High,
                "Migrate shared platform services to reduce release friction and improve observability.",
                "Complete staged rollout for identity and telemetry services.",
                today.AddDays(90),
                "REQ-2026-001",
                DateTime.UtcNow.AddDays(-28),
                "Cross-team modernization effort spanning API, worker services, and deployment automation.").Value,
            Project.Create(
                pulseProjectId,
                "Pulse Executive Analytics",
                jamieId,
                ProjectStatus.InProgress,
                ProjectHealth.AtRisk,
                ProjectPriority.Critical,
                "Deliver executive reporting with daily refreshed KPIs and trend insights.",
                "Stabilize ingestion pipeline and finalize dashboard narratives.",
                today.AddDays(45),
                "REQ-2026-002",
                DateTime.UtcNow.AddDays(-21),
                "Requires data governance review before broad pilot.").Value,
            Project.Create(
                orbitProjectId,
                "Orbit Customer Onboarding",
                alexId,
                ProjectStatus.Waiting,
                ProjectHealth.NotAssessed,
                ProjectPriority.Medium,
                "Shorten time-to-first-value for new enterprise customers.",
                "Approve success metrics and activate trial automation.",
                today.AddDays(120),
                "REQ-2026-003",
                DateTime.UtcNow.AddDays(-7),
                "Waiting on legal templates and support handoff plan.").Value);

        var atlasWorkItemId = WorkItemId.Create("wi-atlas-service-slice").Value;
        var pulseWorkItemId = WorkItemId.Create("wi-pulse-ingestion-hardening").Value;
        var orbitWorkItemId = WorkItemId.Create("wi-orbit-pilot-readiness").Value;

        db.WorkItems.AddRange(
            WorkItem.Create(
                atlasWorkItemId,
                atlasProjectId,
                "Service slice migration",
                "Priya Patel",
                WorkItemStatus.InProgress,
                today.AddDays(21),
                "Test environment parity",
                "Reduce deployment coupling across services",
                "Platform, Security, DevOps",
                "Baseline telemetry and canary policy",
                "Production canary rollout",
                "Identity and telemetry slices run on modernized path for two releases").Value,
            WorkItem.Create(
                pulseWorkItemId,
                pulseProjectId,
                "Ingestion hardening",
                "Mateo Garcia",
                WorkItemStatus.Blocked,
                today.AddDays(14),
                "Vendor schema stability",
                "Improve reliability and reduce manual data fixes",
                "Data Engineering, BI, QA",
                "Contract tests and replay fixtures",
                "48-hour uninterrupted ingest window",
                "No critical incidents over two business days").Value,
            WorkItem.Create(
                orbitWorkItemId,
                orbitProjectId,
                "Pilot readiness checklist",
                "Jamie Owens",
                WorkItemStatus.Waiting,
                today.AddDays(35),
                "Legal template approval",
                "Align onboarding flow and support playbook",
                "Sales Ops, Support, Product",
                "Approved templates and SLA definitions",
                "Pilot go/no-go decision",
                "Checklist fully signed off by all stakeholders").Value);

        db.Activities.AddRange(
            Activity.Create(
                ActivityId.Create("act-atlas-telemetry").Value,
                atlasProjectId,
                atlasWorkItemId,
                "Deploy telemetry sidecar",
                "Priya Patel",
                ActivityStatus.InProgress,
                today.AddDays(10),
                "Canary is stable in staging; production window scheduled.").Value,
            Activity.Create(
                ActivityId.Create("act-pulse-schema-validation").Value,
                pulseProjectId,
                pulseWorkItemId,
                "Validate supplier schema revisions",
                "Mateo Garcia",
                ActivityStatus.NotStarted,
                today.AddDays(6),
                "Awaiting final payload sample for contract verification.").Value,
            Activity.Create(
                ActivityId.Create("act-orbit-success-metrics").Value,
                orbitProjectId,
                orbitWorkItemId,
                "Draft customer success metrics",
                "Jamie Owens",
                ActivityStatus.Done,
                today.AddDays(-2),
                "Initial metric set approved by product and support leads.").Value);

        db.RaidItems.AddRange(
            RaidItem.Create(
                RaidItemId.Create("raid-atlas-risk-observability").Value,
                atlasProjectId,
                RaidItemType.Risk,
                "Gaps in production tracing could mask rollout regressions.",
                "Alex Chen",
                today.AddDays(12)).Value,
            RaidItem.Create(
                RaidItemId.Create("raid-pulse-issue-data-quality").Value,
                pulseProjectId,
                RaidItemType.Issue,
                "Daily import fails when optional columns are missing.",
                "Mateo Garcia",
                today.AddDays(5)).Value,
            RaidItem.Create(
                RaidItemId.Create("raid-orbit-dependency-legal").Value,
                orbitProjectId,
                RaidItemType.Dependency,
                "Pilot launch depends on legal approval of onboarding templates.",
                "Jamie Owens",
                today.AddDays(18)).Value);

        db.SaveChanges();
    }
#endif
}
