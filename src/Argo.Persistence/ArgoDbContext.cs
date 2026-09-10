using Argo.Data.Configurations;
using Argo.Domain.Entities;
using Argo.Domain.Interfaces;
using Argo.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Argo.Data;

/// <summary>
/// Represents the Entity Framework Core database context for Argo domain data.
/// </summary>
/// <remarks>
/// This context maps projects, work items, activities, RAID items, and users,
/// and defines cascade behavior for child records associated with parent entities.
/// </remarks>
/// <param name="options">The options used to configure the database context instance.</param>
internal class ArgoDbContext(DbContextOptions<ArgoDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the project activities tracked by the application.
    /// </summary>
    public DbSet<Activity> Activities => Set<Activity>();

    /// <summary>
    /// Gets the portfolio projects tracked by the application.
    /// </summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>
    /// Gets risk, assumption, issue, and dependency entries associated with projects.
    /// </summary>
    public DbSet<RaidItem> RaidItems => Set<RaidItem>();

    /// <summary>
    /// Gets the user directory entries used for assignment and ownership metadata.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Gets the outbox messages queued for asynchronous domain event dispatch.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Gets project work items used to manage delivery milestones and execution details.
    /// </summary>
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    /// <summary>
    /// Configures entity relationships, keys, and indexes for the Argo data model.
    /// </summary>
    /// <param name="modelBuilder">The builder used to configure EF Core entity mappings.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new WorkItemConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityConfiguration());
        modelBuilder.ApplyConfiguration(new RaidItemConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }

    /// <summary>
    /// Persists pending changes, first draining any domain events recorded on tracked
    /// aggregates into outbox messages so they are guaranteed to be written atomically
    /// with the state change that produced them.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddOutboxMessagesForDomainEvents();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Collects domain events from all tracked aggregates implementing
    /// <see cref="IHasDomainEvents"/>, converts each into a persisted <see cref="OutboxMessage"/>,
    /// and clears the events from their originating aggregate.
    /// </summary>
    private void AddOutboxMessagesForDomainEvents()
    {
        var entitiesWithEvents = ChangeTracker.Entries()
            .Select(entry => entry.Entity)
            .OfType<IHasDomainEvents>()
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                OutboxMessages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName!,
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredOnUtc = DateTime.UtcNow
                });
            }

            entity.ClearDomainEvents();
        }
    }
}
