using Argo.Domain.Entities;
using Argo.Domain.Enums;
using Argo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Argo.Data.Configurations;

/// <summary>
/// Configures EF Core mapping for the <see cref="Project"/> aggregate.
/// </summary>
internal class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => ProjectId.FromTrustedValue(value))
            .ValueGeneratedNever();

        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.Objective).IsRequired();
        builder.Property(p => p.NextMilestone).IsRequired();
        builder.Property(p => p.SourceRequestId).IsRequired();

        builder.Property(p => p.OwnerId)
            .HasConversion(
                ownerId => ownerId.HasValue ? ownerId.Value.Value : null,
                value => value == null ? (UserId?)null : UserId.FromTrustedValue(value))
            .UseCollation("NOCASE");

        builder.HasOne(p => p.Owner)
            .WithMany()
            .HasForeignKey(p => p.OwnerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(p => p.Status)
            .HasConversion(status => status.ToApiString(), value => value.ToProjectStatus())
            .IsRequired();

        builder.Property(p => p.Health)
            .HasConversion(health => health.ToApiString(), value => value.ToProjectHealth())
            .IsRequired();

        builder.Property(p => p.Priority)
            .HasConversion(priority => priority.ToApiString(), value => value.ToProjectPriority())
            .IsRequired();

        builder.Property(p => p.TargetDate).IsRequired();
        builder.Property(p => p.SubmittedAt).IsRequired();
        builder.Property(p => p.IntakeDetails);

        builder.Ignore(p => p.DomainEvents);
    }
}
