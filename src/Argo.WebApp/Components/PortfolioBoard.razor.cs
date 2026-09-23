using Argo.Application.Features.Projects;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Argo.WebApp.Components;

public partial class PortfolioBoard(IDialogService dialogService)
{
    [Parameter] public List<ProjectDto> Projects { get; set; } = [];
    private static readonly string[] Statuses = ["Waiting", "In Progress", "Done"];

    private static readonly Dictionary<string, string> LaneHints = new()
    {
        ["Waiting"] = "Submitted, triaged, or queued for work",
        ["In Progress"] = "Work is underway",
        ["Done"] = "Deliverables accepted",
    };

    private static readonly Dictionary<string, int> PriorityRanks = new()
    {
        ["Needs Triage"] = 5,
        ["Critical"] = 4,
        ["High"] = 3,
        ["Medium"] = 2,
        ["Low"] = 1,
    };

    private static int PriorityRank(string priority) => PriorityRanks.GetValueOrDefault(priority, 0);


    private static Color HealthColor(string health) => health switch
    {
        "On Track" => Color.Success,
        "At Risk" => Color.Warning,
        "Blocked" => Color.Error,
        "Complete" => Color.Info,
        _ => Color.Default,
    };

    private static Color PriorityColor(string priority) => priority switch
    {
        "Critical" => Color.Error,
        "High" => Color.Error,
        "Medium" => Color.Warning,
        "Low" => Color.Default,
        _ => Color.Default,
    };

    private async Task OpenProjectDetails(ProjectDto project)
    {
        var options = new DialogOptions
        {
            FullWidth = false,
            MaxWidth = MaxWidth.False,
            NoHeader = true,
            CloseButton = false
        };

        var parameters = new DialogParameters<PortfolioDetailDialog>
        {
            { d => d.Project, project }
        };

        var dialog = await dialogService.ShowAsync<PortfolioDetailDialog>(parameters, options);
        var dialogResult = await dialog.Result;

    }


    /*    [Parameter] public List<ActivityDTO> Activities { get; set; } = [];
        [Parameter] public List<WorkItemDTO> WorkItems { get; set; } = [];
        [Parameter] public List<RaidItemDTO> RaidItems { get; set; } = [];





        private async Task OpenProjectDetails(ProjectDTO project)
        {
            var options = new DialogOptions
            {
                FullWidth = false,
                MaxWidth = MaxWidth.False,
                NoHeader = true,
                CloseButton = false
            };

            var parameters = new DialogParameters<PortfolioDetailDialog>
            {
                { d => d.Project, project },
                { d => d.Activities, Activities.Where(a => a.ProjectId == project.Id).ToList() },
                { d => d.WorkItems, WorkItems.Where(a => a.ProjectId == project.Id).ToList() },
                { d => d.RaidItems, RaidItems.Where(a => a.ProjectId == project.Id).ToList() }
            };

            await dialogService.ShowAsync<PortfolioDetailDialog>(parameters, options);
        }

    */
}
