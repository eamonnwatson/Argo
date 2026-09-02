namespace Argo.DTO;

/// <summary>
/// Represents a project returned by portfolio APIs.
/// </summary>
/// <param name="Id">The unique project identifier.</param>
/// <param name="Name">The project name.</param>
/// <param name="Owner">The accountable owner.</param>
/// <param name="Status">The current delivery status.</param>
/// <param name="Health">The current health assessment.</param>
/// <param name="Priority">The project priority value.</param>
/// <param name="Objective">The business objective for the project.</param>
/// <param name="NextMilestone">The next key milestone.</param>
/// <param name="TargetDate">The target completion date.</param>
/// <param name="SourceRequestId">The originating intake request identifier.</param>
/// <param name="SubmittedAt">The intake submission timestamp.</param>
public record ProjectDTO(string Id, string Name, string Owner, string Status, string Health, string Priority, string Objective, string NextMilestone, DateOnly TargetDate, string SourceRequestId, DateTime SubmittedAt);

/// <summary>
/// Represents the payload used to create a new project.
/// </summary>
/// <param name="Name">The project name.</param>
/// <param name="Owner">The accountable owner.</param>
/// <param name="Status">The initial delivery status.</param>
/// <param name="Health">The initial health assessment.</param>
/// <param name="Priority">The project priority value.</param>
/// <param name="Objective">The business objective for the project.</param>
/// <param name="NextMilestone">The next key milestone.</param>
/// <param name="TargetDate">The target completion date.</param>
public record ProjectCreateDTO(string Name, string Owner, string Status, string Health, string Priority, string Objective, string NextMilestone, DateOnly TargetDate);
