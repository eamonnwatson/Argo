using Argo.DTO;
using Argo.Models;
using FluentResults;

namespace Argo.Services;

public interface IArgoService
{
    Task<Result<InjestResult>> InjectAsync();
    Task<Result<IReadOnlyCollection<Project>>> GetProjectsAsync();
    Task<Result> SaveProject(string id, ProjectDTO dto);
    Task<Result> DeleteProject(string id);
    Task<Result> SaveWorkItem(string id, WorkItemDTO dto);
    Task<Result> SaveActivity(string id, ActivityDTO dto);
    Task<Result> SaveRaidItem(string id, RaidItemDTO dto);
    Task<Result<InjestResult>> SaveIntakeSubmission(IntakeSubmissionDTO dto);
}
