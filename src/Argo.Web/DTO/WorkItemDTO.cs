namespace Argo.DTO;

/// <summary>
/// Represents a project work item returned by portfolio APIs.
/// </summary>
/// <param name="Id">The unique work item identifier.</param>
/// <param name="ProjectId">The owning project identifier.</param>
/// <param name="Title">The work item title.</param>
/// <param name="Owner">The accountable delivery owner.</param>
/// <param name="Status">The current execution status.</param>
/// <param name="DueDate">The planned completion date.</param>
/// <param name="Dependency">Dependencies that can block execution.</param>
/// <param name="Purpose">The intended business or delivery purpose.</param>
/// <param name="Participants">Contributors involved in delivery.</param>
/// <param name="RequiredInputs">Prerequisite inputs required before work starts.</param>
/// <param name="Milestone">The milestone supported by this work item.</param>
/// <param name="DefinitionOfDone">Acceptance criteria that define completion.</param>
public record WorkItemDTO(string Id, string ProjectId, string Title, string Owner, string Status, DateOnly DueDate, string Dependency, string Purpose, string Participants, string RequiredInputs, string Milestone, string DefinitionOfDone);

/// <summary>
/// Represents the payload used to create a work item.
/// </summary>
/// <param name="ProjectId">The owning project identifier.</param>
/// <param name="Title">The work item title.</param>
/// <param name="Owner">The accountable delivery owner.</param>
/// <param name="Status">The initial execution status.</param>
/// <param name="DueDate">The planned completion date.</param>
/// <param name="Dependency">Dependencies that can block execution.</param>
/// <param name="Purpose">The intended business or delivery purpose.</param>
/// <param name="Participants">Contributors involved in delivery.</param>
/// <param name="RequiredInputs">Prerequisite inputs required before work starts.</param>
/// <param name="Milestone">The milestone supported by this work item.</param>
/// <param name="DefinitionOfDone">Acceptance criteria that define completion.</param>
public record WorkItemCreateDTO(string ProjectId, string Title, string Owner, string Status, DateOnly DueDate, string Dependency, string Purpose, string Participants, string RequiredInputs, string Milestone, string DefinitionOfDone);
