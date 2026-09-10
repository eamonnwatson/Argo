using Argo.Data;
using Microsoft.EntityFrameworkCore;

namespace Argo.Outbox;

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
        var dbContext = scope.ServiceProvider.GetRequiredService<ArgoDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxMessageDispatcher>();

        var now = DateTime.UtcNow;

        var pendingMessages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && (m.NextAttemptUtc == null || m.NextAttemptUtc <= now))
            .OrderBy(m => m.OccurredOnUtc)
            .ToListAsync(cancellationToken);

        foreach (var message in pendingMessages)
        {
            try
            {
                await dispatcher.DispatchAsync(message, cancellationToken);
                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                message.RetryCount++;
                message.Error = ex.Message;

                var backoffSeconds = Math.Min(Math.Pow(2, message.RetryCount), MaxBackoffSeconds);
                message.NextAttemptUtc = DateTime.UtcNow.AddSeconds(backoffSeconds);

                logger.LogWarning(ex, "Failed to process outbox message {MessageId} (attempt {RetryCount}); next attempt at {NextAttemptUtc}.",
                    message.Id, message.RetryCount, message.NextAttemptUtc);
            }
        }

        if (pendingMessages.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);
    }
}
