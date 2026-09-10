using Argo.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Argo.Data.Configurations;

/// <summary>
/// Configures EF Core mapping for the <see cref="OutboxMessage"/> entity.
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).IsRequired();
        builder.Property(m => m.Content).IsRequired();

        builder.HasIndex(m => new { m.ProcessedOnUtc, m.NextAttemptUtc });
    }
}
