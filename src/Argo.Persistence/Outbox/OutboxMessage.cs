namespace Argo.Persistence.Outbox;

/// <summary>
/// Represents a persisted domain event awaiting asynchronous dispatch, guaranteeing that
/// side effects (such as sending email) are captured atomically with the aggregate change
/// that produced them.
/// </summary>
internal class OutboxMessage
{
    /// <summary>
    /// Gets or sets the unique identifier of the outbox message.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the fully-qualified CLR type name of the domain event, used to
    /// select the correct handler and deserializer during dispatch.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON-serialized domain event payload.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when the underlying domain event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the message was successfully processed,
    /// or <see langword="null"/> if it has not yet been processed.
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the most recent error message recorded from a failed processing attempt.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the number of failed processing attempts recorded for this message.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the earliest UTC timestamp at which the next processing attempt may occur.
    /// </summary>
    public DateTime? NextAttemptUtc { get; set; }
}
