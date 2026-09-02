using Argo.DTO;
using Argo.Models;
using FluentResults;

namespace Argo.Services;

/// <summary>
/// Defines application services for managing projects, portfolio items, users,
/// and intake submission ingestion.
/// </summary>
public interface IArgoService
{
    /// <summary>
    /// Executes the configured intake ingestion workflow.
    /// </summary>
    /// <returns>
    /// A result containing summary information for the current ingestion operation.
    /// </returns>
    Task<Result<IngestResult>> InjectAsync();

    /// <summary>
    /// Retrieves all projects with their related portfolio data.
    /// </summary>
    /// <returns>
    /// A result containing the project collection when authorization and retrieval succeed.
    /// </returns>
    Task<Result<IReadOnlyCollection<Project>>> GetProjectsAsync();

    /// <summary>
    /// Retrieves users available for project assignment.
    /// </summary>
    /// <param name="projectManagersOnly">
    /// <see langword="true"/> to return only users marked as project managers; otherwise all users.
    /// </param>
    /// <returns>
    /// A result containing matching user records when retrieval succeeds.
    /// </returns>
    Task<Result<IReadOnlyCollection<User>>> GetUsersAsync(bool projectManagersOnly = false);

    /// <summary>
    /// Creates a new project from the supplied client payload.
    /// </summary>
    /// <param name="dto">The project creation payload.</param>
    /// <returns>A result containing the created project DTO.</returns>
    Task<Result<ProjectDTO>> CreateProject(ProjectCreateDTO dto);

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    /// <param name="id">The identifier of the project to update.</param>
    /// <param name="dto">The replacement project values.</param>
    /// <returns>A result indicating whether the update succeeded.</returns>
    Task<Result> UpdateProject(string id, ProjectDTO dto);

    /// <summary>
    /// Deletes an existing project by identifier.
    /// </summary>
    /// <param name="id">The identifier of the project to delete.</param>
    /// <returns>A result indicating whether the delete operation succeeded.</returns>
    Task<Result> DeleteProject(string id);

    /// <summary>
    /// Creates a new work item.
    /// </summary>
    /// <param name="dto">The work item creation payload.</param>
    /// <returns>A result containing the created work item DTO.</returns>
    Task<Result<WorkItemDTO>> CreateWorkItem(WorkItemCreateDTO dto);

    /// <summary>
    /// Updates an existing work item.
    /// </summary>
    /// <param name="id">The identifier of the work item to update.</param>
    /// <param name="dto">The replacement work item values.</param>
    /// <returns>A result indicating whether the update succeeded.</returns>
    Task<Result> UpdateWorkItem(string id, WorkItemDTO dto);

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    /// <param name="dto">The activity creation payload.</param>
    /// <returns>A result containing the created activity DTO.</returns>
    Task<Result<ActivityDTO>> CreateActivity(ActivityCreateDTO dto);

    /// <summary>
    /// Updates an existing activity.
    /// </summary>
    /// <param name="id">The identifier of the activity to update.</param>
    /// <param name="dto">The replacement activity values.</param>
    /// <returns>A result indicating whether the update succeeded.</returns>
    Task<Result> UpdateActivity(string id, ActivityDTO dto);

    /// <summary>
    /// Creates a new RAID item.
    /// </summary>
    /// <param name="dto">The RAID item creation payload.</param>
    /// <returns>A result containing the created RAID item DTO.</returns>
    Task<Result<RaidItemDTO>> CreateRaidItem(RaidItemCreateDTO dto);

    /// <summary>
    /// Updates an existing RAID item.
    /// </summary>
    /// <param name="id">The identifier of the RAID item to update.</param>
    /// <param name="dto">The replacement RAID values.</param>
    /// <returns>A result indicating whether the update succeeded.</returns>
    Task<Result> UpdateRaidItem(string id, RaidItemDTO dto);

    /// <summary>
    /// Persists an intake submission and creates a corresponding project record.
    /// </summary>
    /// <param name="dto">The intake submission payload to store and map to a new project.</param>
    /// <returns>A result containing the intake request-to-project identifier mapping.</returns>
    Task<Result<IntakeSubmissionResultDTO>> SaveIntakeSubmission(IntakeSubmissionDTO dto);
}
