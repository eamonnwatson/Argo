using Argo.Domain.Entities;
using Argo.Domain.Enums;
using Argo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Argo.Persistence.Configurations;

/// <summary>
/// Configures EF Core mapping for the <see cref="RaidItem"/> entity.
/// </summary>
internal class RaidItemConfiguration : IEntityTypeConfiguration<RaidItem>
{
    public void Configure(EntityTypeBuilder<RaidItem> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => RaidItemId.FromTrustedValue(value))
            .ValueGeneratedNever();

        builder.Property(r => r.ProjectId)
            .HasConversion(id => id.Value, value => ProjectId.FromTrustedValue(value))
            .IsRequired();

        builder.Property(r => r.Description).IsRequired();
        builder.Property(r => r.Owner).IsRequired();

        builder.Property(r => r.Type)
            .HasConversion(type => type.ToApiString(), value => value.ToRaidItemType())
            .IsRequired();

        builder.HasOne<Project>()
            .WithMany(p => p.RaidItems)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(r => r.DomainEvents);
    }
}
