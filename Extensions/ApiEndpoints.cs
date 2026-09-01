using Argo.DTO;
using Argo.Services;

namespace Argo.Extensions;

public static class ApiEndpoints
{
    public static WebApplication MapArgoApi(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1");

        api.MapGet("/portfolio", async (IArgoService argoService) =>
        {
            var result = await argoService.GetProjectsAsync();

            var output = result.Map(
                r =>
                {
                    var projects = r.Select(p => new ProjectDTO(
                        p.Id,
                        p.Name,
                        p.Owner,
                        p.Status,
                        p.Health,
                        p.Priority,
                        p.Objective,
                        p.NextMilestone,
                        p.TargetDate,
                        p.SourceRequestId,
                        p.SubmittedAt
                    )).ToList();

                    var activities = r.SelectMany(p => p.WorkItems).SelectMany(w => w.Activities).Select(a => new ActivityDTO(
                        a.Id,
                        a.ProjectId,
                        a.WorkItemId,
                        a.Title,
                        a.Owner,
                        a.Status,
                        a.DueDate,
                        a.Notes
                    )).ToList();

                    var raidItems = r.SelectMany(p => p.RaidItems).Select(r => new RaidItemDTO(
                        r.Id,
                        r.ProjectId,
                        r.Type,
                        r.Description,
                        r.Owner,
                        r.DueDate
                    )).ToList();

                    var workItems = r.SelectMany(p => p.WorkItems).Select(w => new WorkItemDTO(
                        w!.Id,
                        w.ProjectId,
                        w.Title,
                        w.Owner,
                        w.Status,
                        w.DueDate,
                        w.Dependency,
                        w.Purpose,
                        w.Participants,
                        w.RequiredInputs,
                        w.Milestone,
                        w.DefinitionOfDone
                    )).ToList();

                    return new PortfolioDTO(projects, workItems, activities, raidItems);
                });

            return output.ToResults();
        });

        api.MapPost("/portfolio/ingest", async (IArgoService argoService) =>
        {
            var result = (await argoService.InjectAsync())
                            .Map(result => new InjestDTO(result.Count, result.FirstProjectId));

            return result.ToResults();
        });

        api.MapPut("/projects/{id}", async (string id, ProjectDTO dto, IArgoService argoService) =>
        {
            await argoService.SaveProject(id, dto);
            return Results.NoContent();
        });

        api.MapDelete("/projects/{id}", async (string id, IArgoService argoService) =>
        {
            await argoService.DeleteProject(id);
            return Results.NoContent();
        });

        api.MapPut("/workitems/{id}", async (string id, WorkItemDTO dto, IArgoService argoService) =>
        {
            await argoService.SaveWorkItem(id, dto);
            return Results.NoContent();
        });

        api.MapPut("/activities/{id}", async (string id, ActivityDTO dto, IArgoService argoService) =>
        {
            await argoService.SaveActivity(id, dto);
            return Results.NoContent();
        });

        api.MapPut("/raid/{id}", async (string id, RaidItemDTO dto, IArgoService argoService) =>
        {
            await argoService.SaveRaidItem(id, dto);
            return Results.NoContent();
        });

        api.MapPost("/intake-submissions", async (IntakeSubmissionDTO dto, IArgoService argoService) =>
        {
            var result = await argoService.SaveIntakeSubmission(dto);
            return result.ToResults();
        });

        return app;
    }
}
