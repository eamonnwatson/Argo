using Argo.Data;
using Argo.Domain.Entities;
using Argo.Domain.Enums;
using Argo.Domain.ValueObjects;
using Argo.DTO;
using Argo.Extensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Argo.Services;

/// <summary>
/// Implements Argo application operations for project portfolio management,
/// intake persistence, and user retrieval.
/// </summary>
/// <param name="dbContext">The EF Core context used for all persistence operations.</param>
public class ArgoService(ArgoDbContext dbContext) : IArgoService
{
    private readonly ArgoDbContext dbContext = dbContext;

    /// <summary>
    /// Executes the current ingestion workflow.
    /// </summary>
    /// <returns>
    /// A result containing ingestion summary values.
    /// </returns>
    /// <remarks>
    /// This implementation currently returns a placeholder result and performs no external I/O.
    /// </remarks>
    public async Task<Result<IngestResult>> InjectAsync()
    {
        return Result.Ok(new IngestResult(0, ""));
    }

    /// <summary>
    /// Retrieves all projects and eagerly loads their related work items, activities, and RAID entries.
    /// </summary>
    /// <returns>A result containing the project hierarchy when authorization succeeds.</returns>
    public async Task<Result<IReadOnlyCollection<Project>>> GetProjectsAsync()
    {
        var projects = await dbContext.Projects.AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.WorkItems)
                .ThenInclude(a => a.Activities)
            .Include(p => p.RaidItems)
            .ToListAsync();

        return projects;
    }

    /// <summary>
    /// Retrieves users from the data store, optionally filtered to project managers.
    /// </summary>
    /// <param name="projectManagersOnly">
    /// <see langword="true"/> to include only users flagged as project managers.
    /// </param>
    /// <returns>A result containing user records sorted by display name.</returns>
    public async Task<Result<IReadOnlyCollection<User>>> GetUsersAsync(bool projectManagersOnly = false)
    {
        var query = dbContext.Users.AsNoTracking();

        if (projectManagersOnly)
            query = query.Where(u => u.IsProjectManager);

        var users = await query
            .OrderBy(u => u.DisplayName)
            .ToListAsync();

        return users;
    }

    /// <summary>
    /// Creates a project using values supplied by the client.
    /// </summary>
    /// <param name="dto">The project creation payload.</param>
    /// <returns>A result containing the created project DTO.</returns>
    public async Task<Result<ProjectDTO>> CreateProject(ProjectCreateDTO dto)
    {
        ProjectStatus status;
        ProjectHealth health;
        ProjectPriority priority;

        try
        {
            status = dto.Status.ToProjectStatus();
            health = dto.Health.ToProjectHealth();
            priority = dto.Priority.ToProjectPriority();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result.Fail<ProjectDTO>(APIErrors.ValidationError(ex.Message));
        }

        var ownerResult = await ResolveOwnerAsync(dto.Owner);
        if (ownerResult.IsFailed)
            return ToDomainFailure<ProjectDTO>(ownerResult.Errors);

        var owner = ownerResult.Value;

        var idValue = await GenerateUniqueIdAsync("PRJ", async candidate => await dbContext.Projects.AnyAsync(p => p.Id == ProjectId.Create(candidate).Value));
        var id = ProjectId.Create(idValue).Value;
        var submittedAt = DateTime.Now;

        var projectResult = Project.Create(
            id,
            dto.Name,
            owner?.Id,
            status,
            health,
            priority,
            dto.Objective,
            dto.NextMilestone,
            dto.TargetDate,
            string.Empty,
            submittedAt);

        if (projectResult.IsFailed)
            return ToDomainFailure<ProjectDTO>(projectResult.Errors);

        var project = projectResult.Value;

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        return MapToDto(project, owner?.DisplayName ?? "Unassigned");
    }

    /// <summary>
    /// Updates mutable fields of an existing project.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="dto">The values to apply to the existing project record.</param>
    /// <returns>A result indicating success or the reason for failure.</returns>
    public async Task<Result> UpdateProject(string id, ProjectDTO dto)
    {
        var existing = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == ProjectId.Create(id).Value);
        if (existing is null)
            return Result.Fail(APIErrors.NotFoundError($"Project {id} was not found"));

        ProjectStatus status;
        ProjectHealth health;
        ProjectPriority priority;

        try
        {
            status = dto.Status.ToProjectStatus();
            health = dto.Health.ToProjectHealth();
            priority = dto.Priority.ToProjectPriority();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result.Fail(APIErrors.ValidationError(ex.Message));
        }

        var ownerResult = await ResolveOwnerAsync(dto.Owner);
        if (ownerResult.IsFailed)
            return ToDomainFailure(ownerResult.Errors);

        var updateResult = existing.UpdateDetails(dto.Name, ownerResult.Value?.Id, status, health, priority, dto.Objective, dto.NextMilestone, dto.TargetDate);
        if (updateResult.IsFailed)
            return ToDomainFailure(updateResult.Errors);

        await dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    /// <summary>
    /// Resolves an optional owner display name into the matching <see cref="User"/>,
    /// confirming the referenced user exists.
    /// </summary>
    /// <param name="ownerName">The candidate owner display name, or <see langword="null"/>/empty/"Unassigned" for unassigned.</param>
    /// <returns>A result containing the resolved <see cref="User"/>, or <see langword="null"/> when unassigned.</returns>
    private async Task<Result<User?>> ResolveOwnerAsync(string? ownerName)
    {
        if (string.IsNullOrWhiteSpace(ownerName) || ownerName == "Unassigned")
            return Result.Ok<User?>(null);

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.DisplayName == ownerName);
        if (user is null)
            return Result.Fail<User?>(APIErrors.NotFoundError($"User {ownerName} was not found"));

        return Result.Ok<User?>(user);
    }

    /// <summary>
    /// Deletes a project by identifier.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <returns>A result indicating whether the delete operation completed.</returns>
    public async Task<Result> DeleteProject(string id)
    {
        await dbContext.Projects.Where(p => p.Id == ProjectId.Create(id).Value).ExecuteDeleteAsync();
        return Result.Ok();
    }

    /// <summary>
    /// Creates a work item and stores it in the data store.
    /// </summary>
    /// <param name="dto">The work item creation payload.</param>
    /// <returns>A result containing the created work item DTO.</returns>
    public async Task<Result<WorkItemDTO>> CreateWorkItem(WorkItemCreateDTO dto)
    {
        WorkItemStatus status;
        try
        {
            status = dto.Status.ToWorkItemStatus();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result.Fail<WorkItemDTO>(APIErrors.ValidationError(ex.Message));
        }

        var idValue = await GenerateUniqueIdAsync("WI", async candidate => await dbContext.WorkItems.AnyAsync(w => w.Id == WorkItemId.Create(candidate).Value));
        var id = WorkItemId.Create(idValue).Value;

        var workItemResult = WorkItem.Create(
            id,
            ProjectId.Create(dto.ProjectId).Value,
            dto.Title,
            dto.Owner,
            status,
            dto.DueDate,
            dto.Dependency,
            dto.Purpose,
            dto.Participants,
            dto.RequiredInputs,
            dto.Milestone,
            dto.DefinitionOfDone);

        if (workItemResult.IsFailed)
            return ToDomainFailure<WorkItemDTO>(workItemResult.Errors);

        var workItem = workItemResult.Value;

        dbContext.WorkItems.Add(workItem);
        await dbContext.SaveChangesAsync();

        return MapToDto(workItem);
    }

    /// <summary>
    /// Updates mutable fields of an existing work item.
    /// </summary>
    /// <param name="id">The work item identifier.</param>
    /// <param name="dto">The values to apply to the existing work item record.</param>
    /// <returns>A result indicating success or the reason for failure.</returns>
    public async Task<Result> UpdateWorkItem(string id, WorkItemDTO dto)
    {
        var existing = await dbContext.WorkItems.FirstOrDefaultAsync(w => w.Id == WorkItemId.Create(id).Value);
        if (existing is null)
            return Result.Fail(APIErrors.NotFoundError($"Work item {id} was not found"));

        WorkItemStatus status;
        try
        {
            status = dto.Status.ToWorkItemStatus();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result.Fail(APIErrors.ValidationError(ex.Message));
        }

        var updateResult = existing.UpdateDetails(
            ProjectId.Create(dto.ProjectId).Value,
            dto.Title,
            dto.Owner,
            status,
            dto.DueDate,
            dto.Dependency,
            dto.Purpose,
            dto.Participants,
            dto.RequiredInputs,
            dto.Milestone,
            dto.DefinitionOfDone);

        if (updateResult.IsFailed)
            return ToDomainFailure(updateResult.Errors);

        await dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    /// <summary>
    /// Creates an activity and stores it in the data store.
    /// </summary>
    /// <param name="dto">The activity creation payload.</param>
    /// <returns>A result containing the created activity DTO.</returns>
    public async Task<Result<ActivityDTO>> CreateActivity(ActivityCreateDTO dto)
    {
        ActivityStatus status;
        try
        {
            status = dto.Status.ToActivityStatus();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result.Fail<ActivityDTO>(APIErrors.ValidationError(ex.Message));
        }

        var idValue = await GenerateUniqueIdAsync("ACT", async candidate => await dbContext.Activities.AnyAsync(a => a.Id == ActivityId.Create(candidate).Value));
        var id = ActivityId.Create(idValue).Value;

        var activityResult = Activity.Create(
            id,
            ProjectId.Create(dto.ProjectId).Value,
            WorkItemId.Create(dto.WorkItemId).Value,
            dto.Title,
            dto.Owner,
            status,
            dto.DueDate,
            dto.Notes);

        if (activityResult.IsFailed)
            return ToDomainFailure<ActivityDTO>(activityResult.Errors);

        var activity = activityResult.Value;

        dbContext.Activities.Add(activity);
        await dbContext.SaveChangesAsync();

        return MapToDto(activity);
    }

    /// <summary>
    /// Updates mutable fields of an existing activity.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    /// <param name="dto">The values to apply to the existing activity record.</param>
    /// <returns>A result indicating success or the reason for failure.</returns>
    public async Task<Result> UpdateActivity(string id, ActivityDTO dto)
    {
        var existing = await dbContext.Activities.FirstOrDefaultAsync(a => a.Id == ActivityId.Create(id).Value);
        if (existing is null)
            return Result.Fail(APIErrors.NotFoundError($"Activity {id} was not found"));

        ActivityStatus status;
        try
        {
            status = dto.Status.ToActivityStatus();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result.Fail(APIErrors.ValidationError(ex.Message));
        }

        var updateResult = existing.UpdateDetails(
            ProjectId.Create(dto.ProjectId).Value,
            WorkItemId.Create(dto.WorkItemId).Value,
            dto.Title,
            dto.Owner,
            status,
            dto.DueDate,
            dto.Notes);

        if (updateResult.IsFailed)
            return ToDomainFailure(updateResult.Errors);

        await dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    /// <summary>
    /// Creates a RAID item and stores it in the data store.
    /// </summary>
    /// <param name="dto">The RAID item creation payload.</param>
    /// <returns>A result containing the created RAID item DTO.</returns>
    public async Task<Result<RaidItemDTO>> CreateRaidItem(RaidItemCreateDTO dto)
    {
        RaidItemType type;
        try
        {
            type = dto.Type.ToRaidItemType();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result.Fail<RaidItemDTO>(APIErrors.ValidationError(ex.Message));
        }

        var idValue = await GenerateUniqueIdAsync("RAID", async candidate => await dbContext.RaidItems.AnyAsync(r => r.Id == RaidItemId.Create(candidate).Value));
        var id = RaidItemId.Create(idValue).Value;

        var raidItemResult = RaidItem.Create(
            id,
            ProjectId.Create(dto.ProjectId).Value,
            type,
            dto.Description,
            dto.Owner,
            dto.DueDate);

        if (raidItemResult.IsFailed)
            return ToDomainFailure<RaidItemDTO>(raidItemResult.Errors);

        var raidItem = raidItemResult.Value;

        dbContext.RaidItems.Add(raidItem);
        await dbContext.SaveChangesAsync();

        return MapToDto(raidItem);
    }

    /// <summary>
    /// Updates mutable fields of an existing RAID item.
    /// </summary>
    /// <param name="id">The RAID item identifier.</param>
    /// <param name="dto">The values to apply to the existing RAID item record.</param>
    /// <returns>A result indicating success or the reason for failure.</returns>
    public async Task<Result> UpdateRaidItem(string id, RaidItemDTO dto)
    {
        var existing = await dbContext.RaidItems.FirstOrDefaultAsync(r => r.Id == RaidItemId.Create(id).Value);
        if (existing is null)
            return Result.Fail(APIErrors.NotFoundError($"RAID item {id} was not found"));

        RaidItemType type;
        try
        {
            type = dto.Type.ToRaidItemType();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result.Fail(APIErrors.ValidationError(ex.Message));
        }

        var updateResult = existing.UpdateDetails(ProjectId.Create(dto.ProjectId).Value, type, dto.Description, dto.Owner, dto.DueDate);
        if (updateResult.IsFailed)
            return ToDomainFailure(updateResult.Errors);

        await dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    /// <summary>
    /// Persists the intake submission payload and creates a default triage project.
    /// </summary>
    /// <param name="dto">The intake submission envelope to map into a project record.</param>
    /// <returns>A result containing the generated request and project identifiers.</returns>
    /// <remarks>
    /// Intake details are serialized and stored on the project record so the original
    /// submission remains available during portfolio triage.
    /// </remarks>
    public async Task<Result<IntakeSubmissionResultDTO>> SaveIntakeSubmission(IntakeSubmissionDTO dto)
    {
        var request = dto.Request;

        var requestId = await GenerateUniqueIdAsync("REQ", async candidate => await dbContext.Projects.AnyAsync(p => p.SourceRequestId == candidate));
        var newProjectIdValue = await GenerateUniqueIdAsync("PRJ", async candidate => await dbContext.Projects.AnyAsync(p => p.Id == ProjectId.Create(candidate).Value));
        var title = string.IsNullOrWhiteSpace(request.RequestTitle) ? "Untitled request" : request.RequestTitle;

        var projectResult = Project.Create(
            ProjectId.Create(newProjectIdValue).Value,
            title,
            null,
            ProjectStatus.Waiting,
            ProjectHealth.NotAssessed,
            ProjectPriority.NeedsTriage,
            request.DesiredOutcome ?? request.BusinessProblem ?? request.RequestDescription ?? "Review the submitted business request",
            "Review and triage request",
            DateOnly.Parse(request.DesiredDate),
            requestId,
            DateTime.Now,
            JsonSerializer.Serialize(dto));

        if (projectResult.IsFailed)
            return ToDomainFailure<IntakeSubmissionResultDTO>(projectResult.Errors);

        var project = projectResult.Value;

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        return new IntakeSubmissionResultDTO(requestId, newProjectIdValue);
    }

    /// <summary>
    /// Generates an identifier with the specified prefix and verifies uniqueness via a caller-provided lookup.
    /// </summary>
    /// <param name="prefix">The identifier prefix representing the entity type.</param>
    /// <param name="existsAsync">A delegate that returns whether a candidate identifier already exists.</param>
    /// <param name="maxAttempts">The maximum number of candidate generation attempts.</param>
    /// <returns>A unique identifier candidate.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a unique identifier cannot be generated within <paramref name="maxAttempts"/> attempts.
    /// </exception>
    private static async Task<string> GenerateUniqueIdAsync(string prefix, Func<string, Task<bool>> existsAsync, int maxAttempts = 5)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var candidate = IdGenerator.New(prefix);
            if (!await existsAsync(candidate))
                return candidate;
        }

        throw new InvalidOperationException($"Could not generate a unique id with prefix '{prefix}' after {maxAttempts} attempts.");
    }

    /// <summary>
    /// Translates a collection of domain-layer <see cref="FluentResults.IError"/> instances into
    /// API-facing errors, preserving not-found semantics where present.
    /// </summary>
    private static Result<T> ToDomainFailure<T>(IEnumerable<FluentResults.IError> errors)
    {
        var error = errors.First();
        if (error.HasMetadataKey("NotFound"))
            return Result.Fail<T>(APIErrors.NotFoundError(error.Message));

        return Result.Fail<T>(APIErrors.ValidationError(error.Message));
    }

    /// <summary>
    /// Translates a collection of domain-layer <see cref="FluentResults.IError"/> instances into
    /// API-facing errors, preserving not-found semantics where present.
    /// </summary>
    private static Result ToDomainFailure(IEnumerable<FluentResults.IError> errors)
    {
        var error = errors.First();
        if (error.HasMetadataKey("NotFound"))
            return Result.Fail(APIErrors.NotFoundError(error.Message));

        return Result.Fail(APIErrors.ValidationError(error.Message));
    }

    private static ProjectDTO MapToDto(Project project, string ownerDisplayName) => new(
        project.Id.Value,
        project.Name,
        ownerDisplayName,
        project.Status.ToApiString(),
        project.Health.ToApiString(),
        project.Priority.ToApiString(),
        project.Objective,
        project.NextMilestone,
        project.TargetDate,
        project.SourceRequestId,
        project.SubmittedAt,
        project.IntakeDetails);

    private static WorkItemDTO MapToDto(WorkItem workItem) => new(
        workItem.Id.Value,
        workItem.ProjectId.Value,
        workItem.Title,
        workItem.Owner,
        workItem.Status.ToApiString(),
        workItem.DueDate,
        workItem.Dependency,
        workItem.Purpose,
        workItem.Participants,
        workItem.RequiredInputs,
        workItem.Milestone,
        workItem.DefinitionOfDone);

    private static ActivityDTO MapToDto(Activity activity) => new(
        activity.Id.Value,
        activity.ProjectId.Value,
        activity.WorkItemId.Value,
        activity.Title,
        activity.Owner,
        activity.Status.ToApiString(),
        activity.DueDate,
        activity.Notes);

    private static RaidItemDTO MapToDto(RaidItem raidItem) => new(
        raidItem.Id.Value,
        raidItem.ProjectId.Value,
        raidItem.Type.ToApiString(),
        raidItem.Description,
        raidItem.Owner,
        raidItem.DueDate);
}

/// <summary>
/// Represents a summary of an ingestion operation.
/// </summary>
/// <param name="Count">The number of records processed.</param>
/// <param name="FirstProjectId">The first created project identifier for the operation.</param>
public record IngestResult(int Count, string FirstProjectId);
