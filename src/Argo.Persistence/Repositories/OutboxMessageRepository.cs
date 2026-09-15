using Argo.Application.Outbox;
using Argo.Application.Repositories;
using Argo.Persistence.Common;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Argo.Persistence.Repositories;

/// <summary>
/// EF Core-backed repository for pending outbox messages.
/// </summary>
internal class OutboxMessageRepository(ArgoDbContext dbContext) : BaseRepository, IOutboxMessageRepository
{
    private readonly ArgoDbContext dbContext = dbContext;

    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<PendingOutboxMessage>>> GetPendingAsync(DateTime utcNow, CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () => (IReadOnlyCollection<PendingOutboxMessage>)await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && (m.NextAttemptUtc == null || m.NextAttemptUtc <= utcNow))
            .OrderBy(m => m.OccurredOnUtc)
            .Select(m => new PendingOutboxMessage(m.Id, m.Type, m.Content))
            .ToListAsync(cancellationToken));

    /// <inheritdoc />
    public Task<Result> MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () =>
        {
            var message = await dbContext.OutboxMessages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            if (message is null)
                return;

            message.ProcessedOnUtc = DateTime.UtcNow;
            message.Error = null;
            message.NextAttemptUtc = null;
            await dbContext.SaveChangesAsync(cancellationToken);
        });

    /// <inheritdoc />
    public Task<Result> MarkFailedAsync(Guid id, string error, int retryCount, DateTime nextAttemptUtc, CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () =>
        {
            var message = await dbContext.OutboxMessages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            if (message is null)
                return;

            message.RetryCount = retryCount;
            message.Error = error;
            message.NextAttemptUtc = nextAttemptUtc;
            await dbContext.SaveChangesAsync(cancellationToken);
        });
}
