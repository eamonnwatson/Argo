namespace Argo.Application.Outbox;

/// <summary>
/// Dispatches a deserialized outbox message payload to the handler responsible for its type.
/// </summary>
public interface IOutboxMessageDispatcher
{
    /// <summary>
    /// Dispatches the outbox message's content to the appropriate domain event handler.
    /// </summary>
    /// <param name="message">The outbox message to dispatch.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    Task DispatchAsync(PendingOutboxMessage message, CancellationToken cancellationToken);
}
