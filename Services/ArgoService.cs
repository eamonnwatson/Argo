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

    public async Task<Result<IReadOnlyCollection<User>>> GetUsersAsync()
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

        var users = await dbContext.Users.AsNoTracking()
            .OrderBy(u => u.DisplayName)
            .ToListAsync();

        return users;
    }

    public async Task<Result> SaveProject(string id, ProjectDTO dto)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

        var existing = await dbContext.Projects.FindAsync(id);
        if (existing is null)
        {
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
                SubmittedAt = DateTime.Now
            });
        } 
        else
        {
            existing.Name = dto.Name;
            existing.Owner = dto.Owner;
            existing.Status = dto.Status;
            existing.Health = dto.Health;
            existing.Priority = dto.Priority;
            existing.Objective = dto.Objective;
            existing.NextMilestone = dto.NextMilestone;
            existing.TargetDate = dto.TargetDate;
        }

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

    public async Task<Result> SaveWorkItem(string id, WorkItemDTO dto)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

        var existing = await dbContext.WorkItems.FindAsync(id);
        if (existing is null)
        {
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
        }
        else
        {
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
        }

        await dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> SaveActivity(string id, ActivityDTO dto)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

        var existing = await dbContext.Activities.FindAsync(id);
        if (existing is null)
        {
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
        }
        else
        {
            existing.ProjectId = dto.ProjectId;
            existing.Title = dto.Title;
            existing.Owner = dto.Owner;
            existing.Status = dto.Status;
            existing.DueDate = dto.DueDate;
            existing.Notes = dto.Notes;
            existing.WorkItemId = dto.WorkItemId;
        }

        await dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> SaveRaidItem(string id, RaidItemDTO dto)
    {
        if (!CheckAuthorized().Result)
            return Result.Fail(APIErrors.UnauthroizedError);

        var existing = await dbContext.RaidItems.FindAsync(id);
        if (existing is null)
        {
            await dbContext.RaidItems.AddAsync(new RaidItem
            {
                Id = id,
                ProjectId = dto.ProjectId,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Owner = dto.Owner,
                Type = dto.Type
            });
        }
        else
        {
            existing.ProjectId = dto.ProjectId;
            existing.Owner = dto.Owner;
            existing.DueDate = dto.DueDate;
            existing.Type = dto.Type;
            existing.Description = dto.Description;
        }

        await dbContext.SaveChangesAsync();
        return Result.Ok();
    }
    public async Task<Result<InjestResult>> SaveIntakeSubmission(IntakeSubmissionDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RequestId))
            return Result.Fail("RequestId is required");

        var request = dto.Request;

        var existing = await dbContext.Projects.Where(p => p.SourceRequestId == dto.RequestId).FirstOrDefaultAsync();
        if (existing is not null)
            return Result.Fail("Project already exists");

        var existingProjectIds = await dbContext.Projects.Select(p => p.Id).ToListAsync();

        var newId = NextId("PRJ", existingProjectIds);
        var title = string.IsNullOrWhiteSpace(request.RequestTitle) ? "Untitled request" : request.RequestTitle;

        var project = new Project
        {
            Id = newId,
            Name = title,
            Owner = "Unassigned",
            Status = "Waiting",
            Health = "Not Assigned",
            Priority = "Needs Triage",
            Objective = request.DesiredOutcome ?? request.BusinessProblem ?? request.RequestDescription ?? "Review the submitted business request",
            NextMilestone = "Review and triage request",
            TargetDate = DateOnly.Parse(request.DesiredDate),
            SourceRequestId = dto.RequestId,
            SubmittedAt = DateTime.Now,
            IntakeDetails = JsonSerializer.Serialize(dto),
        };

        await dbContext.Projects.AddAsync(project);
        await dbContext.SaveChangesAsync();

        return new InjestResult(1, newId);
    }

    private static string NextId(string prefix, IEnumerable<string> ids)
    {
        var max = 0;
        foreach (var id in ids)
        {
            var lastPart = id.Split('-').LastOrDefault();
            if (lastPart is not null && int.TryParse(lastPart, out var n) && n > max) 
                max = n;
        }
        return $"{prefix}-{(max + 1).ToString().PadLeft(3, '0')}";
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
