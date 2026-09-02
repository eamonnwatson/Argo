using Argo.DTO;
using Argo.Models;
using Argo.Services;

namespace Argo.Extensions;

/// <summary>
/// Defines Argo minimal API endpoint mappings for portfolio, user, and intake operations.
/// </summary>
public static class ApiEndpoints
{
    /// <summary>
    /// Registers all version 1 Argo API routes on the supplied application.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for fluent pipeline setup.</returns>
    /// <remarks>
    /// Endpoints delegate business operations to <see cref="IArgoService"/> and rely on
    /// <see cref="ResultExtension"/> to map service results into HTTP responses.
    /// </remarks>
    public static WebApplication MapArgoApi(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1");

        api.MapGet("/portfolio", async (IArgoService argoService) =>
            await argoService
                .GetProjectsAsync()
                .MapAsync(MapProjects)
                .ToResultsAsync());

        api.MapPost("/portfolio/ingest", async (IArgoService argoService) =>
            await argoService.InjectAsync()
                .MapAsync(result => new IngestDTO(result.Count, result.FirstProjectId))
                .ToResultsAsync());

        api.MapGet("/users", async (bool? projectManagersOnly, IArgoService argoService) =>
            await argoService.GetUsersAsync(projectManagersOnly ?? false)
                .MapAsync(users => users.Select(u => new UserDTO(u.DomainID, u.DisplayName, u.IsProjectManager)).ToList())
                .ToResultsAsync());

        api.MapPost("/projects", async (ProjectCreateDTO dto, IArgoService argoService) =>
            await argoService.CreateProject(dto).ToResultsAsync());

        api.MapPut("/projects/{id}", async (string id, ProjectDTO dto, IArgoService argoService) =>
            await argoService.UpdateProject(id, dto).ToResultsAsync());

        api.MapDelete("/projects/{id}", async (string id, IArgoService argoService) =>
            await argoService.DeleteProject(id).ToResultsAsync());

        api.MapPost("/workitems", async (WorkItemCreateDTO dto, IArgoService argoService) =>
            await argoService.CreateWorkItem(dto).ToResultsAsync());

        api.MapPut("/workitems/{id}", async (string id, WorkItemDTO dto, IArgoService argoService) =>
            await argoService.UpdateWorkItem(id, dto).ToResultsAsync());

        api.MapPost("/activities", async (ActivityCreateDTO dto, IArgoService argoService) =>
            await argoService.CreateActivity(dto).ToResultsAsync());

        api.MapPut("/activities/{id}", async (string id, ActivityDTO dto, IArgoService argoService) =>
            await argoService.UpdateActivity(id, dto).ToResultsAsync());

        api.MapPost("/raid", async (RaidItemCreateDTO dto, IArgoService argoService) =>
            await argoService.CreateRaidItem(dto).ToResultsAsync());

        api.MapPut("/raid/{id}", async (string id, RaidItemDTO dto, IArgoService argoService) =>
            await argoService.UpdateRaidItem(id, dto).ToResultsAsync());

        api.MapPost("/intake-submissions", async (IntakeSubmissionDTO dto, IArgoService argoService) =>
            await argoService.SaveIntakeSubmission(dto).ToResultsAsync());

        return app;
    }

    /// <summary>
    /// Projects the domain project graph into flattened DTO collections for the portfolio response.
    /// </summary>
    /// <param name="projects">Projects including related work items, activities, and RAID records.</param>
    /// <returns>A portfolio DTO containing project, work item, activity, and RAID collections.</returns>
    private static PortfolioDTO MapProjects(IReadOnlyCollection<Project> projects)
    {
        var projectDto = projects
            .Select(p => new ProjectDTO(p.Id, p.Name, p.Owner, p.Status, p.Health, p.Priority, p.Objective, p.NextMilestone, p.TargetDate, p.SourceRequestId, p.SubmittedAt))
            .ToList();

        // The response exposes child entities as top-level collections so client code can
        // bind and filter each dataset independently without repeatedly traversing the graph.
        var activities = projects
            .SelectMany(p => p.WorkItems)
            .SelectMany(w => w.Activities)
            .Select(a => new ActivityDTO(a.Id, a.ProjectId, a.WorkItemId, a.Title, a.Owner, a.Status, a.DueDate, a.Notes))
            .ToList();

        var raidItems = projects
            .SelectMany(p => p.RaidItems)
            .Select(r => new RaidItemDTO(r.Id, r.ProjectId, r.Type, r.Description, r.Owner, r.DueDate))
            .ToList();

        var workItems = projects
            .SelectMany(p => p.WorkItems)
            .Select(w => new WorkItemDTO(w!.Id, w.ProjectId, w.Title, w.Owner, w.Status, w.DueDate, w.Dependency, w.Purpose, w.Participants, w.RequiredInputs, w.Milestone, w.DefinitionOfDone))
            .ToList();

        return new PortfolioDTO(projectDto, workItems, activities, raidItems);
    }
}
