namespace Argo.DTO;

/// <summary>
/// Represents an activity returned to clients for project execution tracking.
/// </summary>
/// <param name="Id">The unique activity identifier.</param>
/// <param name="ProjectId">The identifier of the parent project.</param>
/// <param name="WorkItemId">The identifier of the parent work item.</param>
/// <param name="Title">The activity title.</param>
/// <param name="Owner">The owner responsible for delivery.</param>
/// <param name="Status">The current activity status value.</param>
/// <param name="DueDate">The planned completion date.</param>
/// <param name="Notes">Additional execution notes.</param>
public record ActivityDTO(string Id, string ProjectId, string WorkItemId, string Title, string Owner, string Status, DateOnly DueDate, string Notes);

/// <summary>
/// Represents the payload used to create a new activity.
/// </summary>
/// <param name="ProjectId">The identifier of the project that owns the activity.</param>
/// <param name="WorkItemId">The identifier of the work item that owns the activity.</param>
/// <param name="Title">The activity title.</param>
/// <param name="Owner">The owner responsible for delivery.</param>
/// <param name="Status">The initial activity status value.</param>
/// <param name="DueDate">The planned completion date.</param>
/// <param name="Notes">Additional execution notes.</param>
public record ActivityCreateDTO(string ProjectId, string WorkItemId, string Title, string Owner, string Status, DateOnly DueDate, string Notes);
