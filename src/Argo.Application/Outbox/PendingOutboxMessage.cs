namespace Argo.Application.Outbox;

/// <summary>
/// Represents the minimal outbox payload needed by the application layer to dispatch and update messages.
/// </summary>
/// <param name="Id">The outbox message identifier.</param>
/// <param name="Type">The fully-qualified CLR type name of the serialized domain event.</param>
/// <param name="Content">The JSON-serialized domain event payload.</param>
public sealed record PendingOutboxMessage(Guid Id, string Type, string Content);
