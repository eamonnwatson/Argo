using Argo.Data;
using Argo.DTO;
using Argo.Extensions;
using Argo.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Argo.Services;

public class ArgoService(ArgoDbContext dbContext, IHttpContextAccessor httpContextAccessor) : IArgoService
{
    private readonly ArgoDbContext dbContext = dbContext;
    private readonly HttpContext? httpContext = httpContextAccessor.HttpContext;

    public async Task<Result<InjestResult>> InjectAsync()
    {
        return Result.Ok(new InjestResult(0, ""));
    }

    public async Task<Result<IReadOnlyCollection<Project>>> GetProjectsAsync()
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

        var projects = await dbContext.Projects.AsNoTracking()
            .Include(p => p.WorkItems)
                .ThenInclude(a => a.Activities)
            .Include(p => p.RaidItems)
            .ToListAsync();

        return projects;
    }

    public async Task<Result<IReadOnlyCollection<User>>> GetUsersAsync(bool projectManagersOnly = false)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

        var query = dbContext.Users.AsNoTracking();

        if (projectManagersOnly)
            query = query.Where(u => u.IsProjectManager);

        var users = await query
            .OrderBy(u => u.DisplayName)
            .ToListAsync();

        return users;
    }

    public async Task<Result<ProjectDTO>> CreateProject(ProjectCreateDTO dto)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

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

    public async Task<Result> UpdateProject(string id, ProjectDTO dto)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

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

    public async Task<Result> DeleteProject(string id)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

        await dbContext.Projects.Where(p => p.Id == id).ExecuteDeleteAsync();
        return Result.Ok();
    }

    public async Task<Result<WorkItemDTO>> CreateWorkItem(WorkItemCreateDTO dto)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

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

    public async Task<Result> UpdateWorkItem(string id, WorkItemDTO dto)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

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

    public async Task<Result<ActivityDTO>> CreateActivity(ActivityCreateDTO dto)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

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

    public async Task<Result> UpdateActivity(string id, ActivityDTO dto)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

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

    public async Task<Result<RaidItemDTO>> CreateRaidItem(RaidItemCreateDTO dto)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

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

    public async Task<Result> UpdateRaidItem(string id, RaidItemDTO dto)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

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

    private async Task<bool> CheckAuthorized()
    {
        var user = httpContext?.User?.Identity?.Name;
        if (user is null)
            return false;

        var separatorIndex = user.LastIndexOf('\\');
        if (separatorIndex >= 0)
            user = user[(separatorIndex + 1)..];

        var dbUser = await dbContext.Users.Where(u => u.DomainID.ToUpper() == user.ToUpper()).FirstOrDefaultAsync();

        return dbUser is not null;
    }
}

public record InjestResult(int Count, string FirstProjectId);
