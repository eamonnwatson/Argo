using Argo.Application.Outbox;
using FluentResults;

namespace Argo.Application.Repositories;

/// <summary>
/// Provides application-facing access to pending outbox messages.
/// </summary>
public interface IOutboxMessageRepository
{
    /// <summary>
    /// Retrieves all outbox messages that are ready to be dispatched.
    /// </summary>
    Task<Result<IReadOnlyCollection<PendingOutboxMessage>>> GetPendingAsync(DateTime utcNow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified outbox message as successfully processed.
    /// </summary>
    Task<Result> MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified outbox message as failed and schedules the next retry.
    /// </summary>
    Task<Result> MarkFailedAsync(Guid id, string error, int retryCount, DateTime nextAttemptUtc, CancellationToken cancellationToken = default);
}
