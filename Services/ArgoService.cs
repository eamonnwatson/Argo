using Argo.Data;
using Argo.DTO;
using Argo.Extensions;
using Argo.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Argo.Services;

/// <summary>
/// Implements Argo application operations for project portfolio management,
/// intake persistence, and user retrieval.
/// </summary>
/// <param name="dbContext">The EF Core context used for all persistence operations.</param>
/// <param name="httpContextAccessor">Provides access to the current request user for authorization checks.</param>
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
        var id = await GenerateUniqueIdAsync("PRJ", async candidate => await dbContext.Projects.AnyAsync(p => p.Id == candidate));
        var submittedAt = DateTime.Now;

        dbContext.Projects.Add(new Project
        {
            Id = id,
            Name = dto.Name,
            Owner = dto.Owner,
            Status = dto.Status,
            Health = dto.Health,
            Priority = dto.Priority,
            Objective = dto.Objective,
            NextMilestone = dto.NextMilestone,
            TargetDate = dto.TargetDate,
            SourceRequestId = string.Empty,
            SubmittedAt = submittedAt
        });

        await dbContext.SaveChangesAsync();

        return new ProjectDTO(id, dto.Name, dto.Owner, dto.Status, dto.Health, dto.Priority, dto.Objective, dto.NextMilestone, dto.TargetDate, string.Empty, submittedAt);
    }

    /// <summary>
    /// Updates mutable fields of an existing project.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="dto">The values to apply to the existing project record.</param>
    /// <returns>A result indicating success or the reason for failure.</returns>
    public async Task<Result> UpdateProject(string id, ProjectDTO dto)
    {
        var existing = await dbContext.Projects.FindAsync(id);
        if (existing is null)
            return Result.Fail(APIErrors.NotFoundError($"Project {id} was not found"));

        existing.Name = dto.Name;
        existing.Owner = dto.Owner;
        existing.Status = dto.Status;
        existing.Health = dto.Health;
        existing.Priority = dto.Priority;
        existing.Objective = dto.Objective;
        existing.NextMilestone = dto.NextMilestone;
        existing.TargetDate = dto.TargetDate;

        await dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    /// <summary>
    /// Deletes a project by identifier.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <returns>A result indicating whether the delete operation completed.</returns>
    public async Task<Result> DeleteProject(string id)
    {
        await dbContext.Projects.Where(p => p.Id == id).ExecuteDeleteAsync();
        return Result.Ok();
    }

    /// <summary>
    /// Creates a work item and stores it in the data store.
    /// </summary>
    /// <param name="dto">The work item creation payload.</param>
    /// <returns>A result containing the created work item DTO.</returns>
    public async Task<Result<WorkItemDTO>> CreateWorkItem(WorkItemCreateDTO dto)
    {
        var id = await GenerateUniqueIdAsync("WI", async candidate => await dbContext.WorkItems.AnyAsync(w => w.Id == candidate));

        await dbContext.WorkItems.AddAsync(new WorkItem
        {
            Id = id,
            ProjectId = dto.ProjectId,
            Title = dto.Title,
            Owner = dto.Owner,
            Status = dto.Status,
            DueDate = dto.DueDate,
            Dependency = dto.Dependency,
            Purpose = dto.Purpose,
            Participants = dto.Participants,
            RequiredInputs = dto.RequiredInputs,
            Milestone = dto.Milestone,
            DefinitionOfDone = dto.DefinitionOfDone
        });

        await dbContext.SaveChangesAsync();

        return new WorkItemDTO(id, dto.ProjectId, dto.Title, dto.Owner, dto.Status, dto.DueDate, dto.Dependency, dto.Purpose, dto.Participants, dto.RequiredInputs, dto.Milestone, dto.DefinitionOfDone);
    }

    /// <summary>
    /// Updates mutable fields of an existing work item.
    /// </summary>
    /// <param name="id">The work item identifier.</param>
    /// <param name="dto">The values to apply to the existing work item record.</param>
    /// <returns>A result indicating success or the reason for failure.</returns>
    public async Task<Result> UpdateWorkItem(string id, WorkItemDTO dto)
    {
        var existing = await dbContext.WorkItems.FindAsync(id);
        if (existing is null)
            return Result.Fail(APIErrors.NotFoundError($"Work item {id} was not found"));

        existing.ProjectId = dto.ProjectId;
        existing.Title = dto.Title;
        existing.Owner = dto.Owner;
        existing.Status = dto.Status;
        existing.DueDate = dto.DueDate;
        existing.Dependency = dto.Dependency;
        existing.Purpose = dto.Purpose;
        existing.Participants = dto.Participants;
        existing.RequiredInputs = dto.RequiredInputs;
        existing.Milestone = dto.Milestone;
        existing.DefinitionOfDone = dto.DefinitionOfDone;

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
        var id = await GenerateUniqueIdAsync("ACT", async candidate => await dbContext.Activities.AnyAsync(a => a.Id == candidate));

        await dbContext.Activities.AddAsync(new Activity
        {
            Id = id,
            ProjectId = dto.ProjectId,
            Title = dto.Title,
            Owner = dto.Owner,
            Status = dto.Status,
            DueDate = dto.DueDate,
            Notes = dto.Notes,
            WorkItemId = dto.WorkItemId
        });

        await dbContext.SaveChangesAsync();

        return new ActivityDTO(id, dto.ProjectId, dto.WorkItemId, dto.Title, dto.Owner, dto.Status, dto.DueDate, dto.Notes);
    }

    /// <summary>
    /// Updates mutable fields of an existing activity.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    /// <param name="dto">The values to apply to the existing activity record.</param>
    /// <returns>A result indicating success or the reason for failure.</returns>
    public async Task<Result> UpdateActivity(string id, ActivityDTO dto)
    {
        var existing = await dbContext.Activities.FindAsync(id);
        if (existing is null)
            return Result.Fail(APIErrors.NotFoundError($"Activity {id} was not found"));

        existing.ProjectId = dto.ProjectId;
        existing.Title = dto.Title;
        existing.Owner = dto.Owner;
        existing.Status = dto.Status;
        existing.DueDate = dto.DueDate;
        existing.Notes = dto.Notes;
        existing.WorkItemId = dto.WorkItemId;

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
        var id = await GenerateUniqueIdAsync("RAID", async candidate => await dbContext.RaidItems.AnyAsync(r => r.Id == candidate));

        await dbContext.RaidItems.AddAsync(new RaidItem
        {
            Id = id,
            ProjectId = dto.ProjectId,
            Description = dto.Description,
            DueDate = dto.DueDate,
            Owner = dto.Owner,
            Type = dto.Type
        });

        await dbContext.SaveChangesAsync();

        return new RaidItemDTO(id, dto.ProjectId, dto.Type, dto.Description, dto.Owner, dto.DueDate);
    }

    /// <summary>
    /// Updates mutable fields of an existing RAID item.
    /// </summary>
    /// <param name="id">The RAID item identifier.</param>
    /// <param name="dto">The values to apply to the existing RAID item record.</param>
    /// <returns>A result indicating success or the reason for failure.</returns>
    public async Task<Result> UpdateRaidItem(string id, RaidItemDTO dto)
    {
        var existing = await dbContext.RaidItems.FindAsync(id);
        if (existing is null)
            return Result.Fail(APIErrors.NotFoundError($"RAID item {id} was not found"));

        existing.ProjectId = dto.ProjectId;
        existing.Owner = dto.Owner;
        existing.DueDate = dto.DueDate;
        existing.Type = dto.Type;
        existing.Description = dto.Description;

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
        var newProjectId = await GenerateUniqueIdAsync("PRJ", async candidate => await dbContext.Projects.AnyAsync(p => p.Id == candidate));
        var title = string.IsNullOrWhiteSpace(request.RequestTitle) ? "Untitled request" : request.RequestTitle;

        var project = new Project
        {
            Id = newProjectId,
            Name = title,
            Owner = "Unassigned",
            Status = "Waiting",
            Health = "Not Assigned",
            Priority = "Needs Triage",
            Objective = request.DesiredOutcome ?? request.BusinessProblem ?? request.RequestDescription ?? "Review the submitted business request",
            NextMilestone = "Review and triage request",
            TargetDate = DateOnly.Parse(request.DesiredDate),
            SourceRequestId = requestId,
            SubmittedAt = DateTime.Now,
            IntakeDetails = JsonSerializer.Serialize(dto),
        };

        await dbContext.Projects.AddAsync(project);
        await dbContext.SaveChangesAsync();

        return new IntakeSubmissionResultDTO(requestId, newProjectId);
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

    }

/// <summary>
/// Represents a summary of an ingestion operation.
/// </summary>
/// <param name="Count">The number of records processed.</param>
/// <param name="FirstProjectId">The first created project identifier for the operation.</param>
public record IngestResult(int Count, string FirstProjectId);
