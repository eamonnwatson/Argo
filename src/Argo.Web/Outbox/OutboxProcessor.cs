using Argo.Application.Outbox;
using Argo.Application.Repositories;

namespace Argo.Web.Outbox;

/// <summary>
/// Background service that polls for pending outbox messages and dispatches them,
/// applying exponential backoff retry on failure.
/// </summary>
/// <param name="scopeFactory">Used to create a per-poll dependency injection scope.</param>
/// <param name="logger">The logger used to record processing failures.</param>
public class OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);
    private const int MaxBackoffSeconds = 300;

    private readonly IServiceScopeFactory scopeFactory = scopeFactory;
    private readonly ILogger<OutboxProcessor> logger = logger;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        do
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unhandled error while processing outbox messages.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxMessageDispatcher>();

        var now = DateTime.UtcNow;
        var pendingMessagesResult = await outboxRepository.GetPendingAsync(now, cancellationToken);
        if (pendingMessagesResult.IsFailed)
        {
            logger.LogWarning("Unable to load pending outbox messages: {Error}", pendingMessagesResult.Errors.FirstOrDefault()?.Message ?? "unknown error");
            return;
        }

        foreach (var message in pendingMessagesResult.Value)
        {
            try
            {
                await dispatcher.DispatchAsync(message, cancellationToken);
                var processedResult = await outboxRepository.MarkProcessedAsync(message.Id, cancellationToken);
                if (processedResult.IsFailed)
                    logger.LogWarning("Failed to mark outbox message {MessageId} as processed: {Error}", message.Id, processedResult.Errors.FirstOrDefault()?.Message ?? "unknown error");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var retryCount = 1;
                var backoffSeconds = Math.Min(Math.Pow(2, retryCount), MaxBackoffSeconds);
                var nextAttemptUtc = DateTime.UtcNow.AddSeconds(backoffSeconds);

                var failedResult = await outboxRepository.MarkFailedAsync(message.Id, ex.Message, retryCount, nextAttemptUtc, cancellationToken);
                if (failedResult.IsFailed)
                    logger.LogWarning("Failed to mark outbox message {MessageId} as failed: {Error}", message.Id, failedResult.Errors.FirstOrDefault()?.Message ?? "unknown error");

                logger.LogWarning(ex, "Failed to process outbox message {MessageId} (attempt {RetryCount}); next attempt at {NextAttemptUtc}.",
                    message.Id, retryCount, nextAttemptUtc);
            }
        }
    }
}
