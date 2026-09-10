using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Argo.Data.Configurations;

/// <summary>
/// Configures EF Core mapping for the <see cref="User"/> entity.
/// </summary>
internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => UserId.FromTrustedValue(value))
            .HasColumnName("DomainID")
            .ValueGeneratedNever();

        builder.Property(u => u.DisplayName).IsRequired();

        builder.Property(u => u.Email);

        builder.Ignore(u => u.DomainEvents);
    }
}
