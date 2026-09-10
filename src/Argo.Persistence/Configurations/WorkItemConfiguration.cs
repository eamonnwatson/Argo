using Argo.Domain.Entities;
using Argo.Domain.Enums;
using Argo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Argo.Data.Configurations;

/// <summary>
/// Configures EF Core mapping for the <see cref="WorkItem"/> entity.
/// </summary>
internal class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasConversion(id => id.Value, value => WorkItemId.FromTrustedValue(value))
            .ValueGeneratedNever();

        builder.Property(w => w.ProjectId)
            .HasConversion(id => id.Value, value => ProjectId.FromTrustedValue(value))
            .IsRequired();

        builder.Property(w => w.Title).IsRequired();
        builder.Property(w => w.Owner).IsRequired();
        builder.Property(w => w.Dependency).IsRequired();
        builder.Property(w => w.Purpose).IsRequired();
        builder.Property(w => w.Participants).IsRequired();
        builder.Property(w => w.RequiredInputs).IsRequired();
        builder.Property(w => w.Milestone).IsRequired();
        builder.Property(w => w.DefinitionOfDone).IsRequired();

        builder.Property(w => w.Status)
            .HasConversion(status => status.ToApiString(), value => value.ToWorkItemStatus())
            .IsRequired();

        builder.HasOne<Project>()
            .WithMany(p => p.WorkItems)
            .HasForeignKey(w => w.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(WorkItem.Activities))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(w => w.DomainEvents);
    }
}
