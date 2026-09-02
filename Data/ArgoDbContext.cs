using Argo.Models;
using Microsoft.EntityFrameworkCore;

namespace Argo.Data;

/// <summary>
/// Represents the Entity Framework Core database context for Argo domain data.
/// </summary>
/// <remarks>
/// This context maps projects, work items, activities, RAID items, and users,
/// and defines cascade behavior for child records associated with parent entities.
/// </remarks>
/// <param name="options">The options used to configure the database context instance.</param>
public class ArgoDbContext(DbContextOptions<ArgoDbContext> options) : DbContext(options)
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
    /// Gets project work items used to manage delivery milestones and execution details.
    /// </summary>
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    /// <summary>
    /// Configures entity relationships, keys, and indexes for the Argo data model.
    /// </summary>
    /// <param name="modelBuilder">The builder used to configure EF Core entity mappings.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>()
            .HasOne(a => a.WorkItem)
            .WithMany(w => w.Activities)
            .HasForeignKey(a => a.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Source request IDs are used to correlate intake submissions to created projects.
        modelBuilder.Entity<Project>().HasIndex(p => p.SourceRequestId);

        modelBuilder.Entity<RaidItem>()
            .HasOne(r => r.Project)
            .WithMany(p => p.RaidItems)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>().HasKey(u => u.DomainID);

        modelBuilder.Entity<WorkItem>()
            .HasOne(w => w.Project)
            .WithMany(p => p.WorkItems)
            .HasForeignKey(w => w.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
