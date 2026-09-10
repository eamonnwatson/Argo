using Argo.Domain.Events;
using Argo.Notifications;
using System.Text.Json;

namespace Argo.Outbox;

/// <summary>
/// Maps outbox message types to their corresponding domain event handlers. Unknown
/// message types are logged and treated as handled to avoid poison-message retry loops.
/// </summary>
/// <param name="notificationHandler">The handler for <see cref="ProjectManagerChanged"/> events.</param>
/// <param name="logger">The logger used to record unrecognized message types.</param>
public class OutboxMessageDispatcher(ProjectManagerChangedNotificationHandler notificationHandler, ILogger<OutboxMessageDispatcher> logger) : IOutboxMessageDispatcher
{
    private readonly ProjectManagerChangedNotificationHandler notificationHandler = notificationHandler;
    private readonly ILogger<OutboxMessageDispatcher> logger = logger;

    /// <inheritdoc />
    public async Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var eventType = Type.GetType(message.Type);

        if (eventType == typeof(ProjectManagerChanged))
        {
            var domainEvent = JsonSerializer.Deserialize<ProjectManagerChanged>(message.Content);
            if (domainEvent is null)
            {
                logger.LogWarning("Outbox message {MessageId} of type {Type} could not be deserialized.", message.Id, message.Type);
                return;
            }

            await notificationHandler.HandleAsync(domainEvent, cancellationToken);
            return;
        }

        logger.LogWarning("Outbox message {MessageId} has unrecognized type {Type}; marking as processed without dispatch.", message.Id, message.Type);
    }
}
