using Argo.Models;
using Microsoft.EntityFrameworkCore;

namespace Argo.Data;

public class ArgoDbContext(DbContextOptions<ArgoDbContext> options) : DbContext(options)
{
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<RaidItem> RaidItems => Set<RaidItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>()
            .HasOne(a => a.WorkItem)
            .WithMany(w => w.Activities)
            .HasForeignKey(a => a.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

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
