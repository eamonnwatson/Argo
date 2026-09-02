using Argo.DTO;
using Argo.Models;
using FluentResults;

namespace Argo.Services;

public interface IArgoService
{
    Task<Result<InjestResult>> InjectAsync();
    Task<Result<IReadOnlyCollection<Project>>> GetProjectsAsync();
    Task<Result<IReadOnlyCollection<User>>> GetUsersAsync(bool projectManagersOnly = false);
    Task<Result<ProjectDTO>> CreateProject(ProjectCreateDTO dto);
    Task<Result> UpdateProject(string id, ProjectDTO dto);
    Task<Result> DeleteProject(string id);
    Task<Result<WorkItemDTO>> CreateWorkItem(WorkItemCreateDTO dto);
    Task<Result> UpdateWorkItem(string id, WorkItemDTO dto);
    Task<Result<ActivityDTO>> CreateActivity(ActivityCreateDTO dto);
    Task<Result> UpdateActivity(string id, ActivityDTO dto);
    Task<Result<RaidItemDTO>> CreateRaidItem(RaidItemCreateDTO dto);
    Task<Result> UpdateRaidItem(string id, RaidItemDTO dto);
    Task<Result<IntakeSubmissionResultDTO>> SaveIntakeSubmission(IntakeSubmissionDTO dto);
}
