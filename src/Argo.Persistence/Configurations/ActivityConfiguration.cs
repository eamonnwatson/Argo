using Argo.Domain.Entities;
using Argo.Domain.Enums;
using Argo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Argo.Data.Configurations;

/// <summary>
/// Configures EF Core mapping for the <see cref="Activity"/> entity.
/// </summary>
internal class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => ActivityId.FromTrustedValue(value))
            .ValueGeneratedNever();

        builder.Property(a => a.ProjectId)
            .HasConversion(id => id.Value, value => ProjectId.FromTrustedValue(value))
            .IsRequired();

        builder.Property(a => a.WorkItemId)
            .HasConversion(id => id.Value, value => WorkItemId.FromTrustedValue(value))
            .IsRequired();

        builder.Property(a => a.Title).IsRequired();
        builder.Property(a => a.Owner).IsRequired();
        builder.Property(a => a.Notes).IsRequired();

        builder.Property(a => a.Status)
            .HasConversion(status => status.ToApiString(), value => value.ToActivityStatus())
            .IsRequired();

        builder.HasOne<WorkItem>()
            .WithMany(w => w.Activities)
            .HasForeignKey(a => a.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(a => a.DomainEvents);
    }
}
