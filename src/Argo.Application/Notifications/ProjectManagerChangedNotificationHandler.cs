using Argo.Application.Repositories;
using Argo.Domain.Enums;
using Argo.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Argo.Application.Notifications;

/// <summary>
/// Handles <see cref="ProjectManagerChanged"/> domain events by emailing the newly
/// assigned project manager a concise summary of the project. Unassignment (no new
/// owner) and missing recipient data are treated as handled with no email sent.
/// </summary>
/// <param name="projectRepository">The repository used to load project summary data.</param>
/// <param name="userRepository">The repository used to load user summary data.</param>
/// <param name="emailSender">The email sender used to deliver the notification.</param>
/// <param name="logger">The logger used to record skipped or unresolved notifications.</param>
public class ProjectManagerChangedNotificationHandler(IProjectRepository projectRepository, IUserRepository userRepository, IEmailSender emailSender, ILogger<ProjectManagerChangedNotificationHandler> logger)
{
    private readonly IProjectRepository projectRepository = projectRepository;
    private readonly IUserRepository userRepository = userRepository;
    private readonly IEmailSender emailSender = emailSender;
    private readonly ILogger<ProjectManagerChangedNotificationHandler> logger = logger;

    /// <summary>
    /// Processes the supplied event, sending an assignment notification email when applicable.
    /// </summary>
    /// <param name="domainEvent">The manager-change event to handle.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    public async Task HandleAsync(ProjectManagerChanged domainEvent, CancellationToken cancellationToken)
    {
        if (domainEvent.NewOwnerId is null)
        {
            logger.LogInformation("Project {ProjectId} was unassigned; no notification email sent.", domainEvent.ProjectId.Value);
            return;
        }

        var projectResult = await projectRepository.GetByIdAsync(domainEvent.ProjectId, cancellationToken);
        if (projectResult.IsFailed || projectResult.Value is null)
        {
            logger.LogWarning("Project {ProjectId} was not found; skipping manager-change notification.", domainEvent.ProjectId.Value);
            return;
        }

        var project = projectResult.Value;

        var userResult = await userRepository.GetByIdAsync(domainEvent.NewOwnerId.Value, cancellationToken);
        if (userResult.IsFailed || userResult.Value is null)
        {
            logger.LogWarning("User {UserId} was not found; skipping manager-change notification for project {ProjectId}.", domainEvent.NewOwnerId.Value.Value, domainEvent.ProjectId.Value);
            return;
        }

        var user = userResult.Value;

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            logger.LogWarning("User {UserId} has no email address; skipping manager-change notification for project {ProjectId}.", user.Id.Value, domainEvent.ProjectId.Value);
            return;
        }

        var subject = $"You have been assigned as project manager for {project.Name}";
        var body =
$"""
Hello {user.DisplayName},

You have been assigned as project manager for the following project:

- Project: {project.Name} ({project.Id.Value})
- Status: {project.Status.ToApiString()}
- Priority: {project.Priority.ToApiString()}
- Target date: {project.TargetDate:d}

Please review the project in Argo.
""";

        await emailSender.SendAsync(user.Email, subject, body, cancellationToken);
    }
}
