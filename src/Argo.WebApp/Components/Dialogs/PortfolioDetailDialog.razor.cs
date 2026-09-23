using Argo.Application.Common.Messaging;
using Argo.Application.Features.Projects;
using Argo.Application.Features.RaidItems;
using Argo.Application.Features.RaidItems.Queries.GetRaidItems;
using Argo.Application.Features.WorkItems;
using Argo.Application.Features.WorkItems.Queries.GetWorkItems;
using Argo.Domain.Enums;
using Argo.WebApp.Infrastructure;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Argo.WebApp.Components;

public partial class PortfolioDetailDialog
{
    [Inject] private IDispatcher Dispatcher { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance? MudDialog { get; set; }
    [Parameter] public ProjectDto Project { get; set; } = default!;
    private List<WorkItemDto> workItems = [];
    private List<RaidItemDto> raidItems = [];

    private int WavesTotal => workItems.Count;
    private int WavesDone => workItems.Count(w => w.Status == WorkItemStatus.Done);
    private int WavesCompletedPercent => WavesTotal == 0 ? 0 : (int)Math.Round(100.0 * WavesDone / WavesTotal);
    private void ToggleIntakeDetails() => intakeExpanded = !intakeExpanded;
    private bool intakeExpanded; 
    private readonly HashSet<string> expandedWorkItemIds = [];

    protected override async Task OnParametersSetAsync()
    {
        var workItemsResult = await Dispatcher.SendAsync(new GetWorkItemsQuery(Project.Id));

        if (workItemsResult.ShowErrorsIfFailed(Snackbar))
            return;

        workItems = workItemsResult.Value.ToList();

        var raidItemsResult = await Dispatcher.SendAsync(new GetRaidItemsQuery(Project.Id));

        if (raidItemsResult.ShowErrorsIfFailed(Snackbar))
            return;

        raidItems = raidItemsResult.Value.ToList();

    }
    private void ToggleWorkItem(string id)
    {
        if (!expandedWorkItemIds.Add(id))
        {
            expandedWorkItemIds.Remove(id);
        }
    }

    private (int Done, int Total) ActivityProgress(string workItemId)
    {
        var forItem = workItems.FirstOrDefault(w => w.WorkItemId == workItemId)?.Activities.ToList() ?? [];

        return (forItem.Count(a => a.Status == ActivityStatus.Done), forItem.Count);
    }

    private void CloseDialog() => MudDialog?.Cancel();

    private Task EditProject()
    {
        return Task.CompletedTask;
    }


    private static Color HealthColor(string health) => health switch
    {
        "On Track" => Color.Success,
        "At Risk" => Color.Warning,
        "Blocked" => Color.Error,
        "Complete" => Color.Info,
        "Not Assessed" => Color.Dark,
        _ => Color.Default,
    };

    private static Color ProjectStatusColor(string status) => status switch
    {
        "Done" => Color.Success,
        "In Progress" => Color.Warning,
        "Blocked" => Color.Error,
        "Waiting" => Color.Info,
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

    private static Color WorkStatusColor(WorkItemStatus status) => status switch
    {
        WorkItemStatus.Done => Color.Success,
        WorkItemStatus.InProgress => Color.Info,
        WorkItemStatus.Blocked => Color.Error,
        WorkItemStatus.Waiting => Color.Warning,
        _ => Color.Default,
    };




    /*    [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private IArgoService ArgoService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        [CascadingParameter] private IMudDialogInstance? MudDialog { get; set; }


        [Parameter] public ProjectDTO Project { get; set; } = null!;
        [Parameter] public List<WorkItemDTO> WorkItems { get; set; } = [];
        [Parameter] public List<ActivityDTO> Activities { get; set; } = [];
        [Parameter] public List<RaidItemDTO> RaidItems { get; set; } = [];
        [Parameter] public List<UserDTO> Users { get; set; } = [];

        private readonly HashSet<string> expandedWorkItemIds = [];
        private bool intakeExpanded;

        private int WavesTotal => WorkItems.Count;
        private int WavesDone => WorkItems.Count(w => w.Status == "Done");
        private int WavesCompletedPercent => WavesTotal == 0 ? 0 : (int)Math.Round(100.0 * WavesDone / WavesTotal);

        private void ToggleIntakeDetails() => intakeExpanded = !intakeExpanded;

        private void ToggleWorkItem(string id)
        {
            if (!expandedWorkItemIds.Add(id))
            {
                expandedWorkItemIds.Remove(id);
            }
        }

        private (int Done, int Total) ActivityProgress(string workItemId)
        {
            var forItem = Activities.Where(a => a.WorkItemId == workItemId).ToList();
            return (forItem.Count(a => a.Status == "Done"), forItem.Count);
        }

        private static Color HealthColor(string health) => health switch
        {
            "On Track" => Color.Success,
            "At Risk" => Color.Warning,
            "Blocked" => Color.Error,
            "Complete" => Color.Info,
            "Not Assessed" => Color.Dark,
            _ => Color.Default,
        };

        private static Color ProjectStatusColor(string status) => status switch
        {
            "Done" => Color.Success,
            "In Progress" => Color.Warning,
            "Blocked" => Color.Error,
            "Waiting" => Color.Info,
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

        private static Color WorkStatusColor(string status) => status switch
        {
            "Done" => Color.Success,
            "In Progress" => Color.Info,
            "Blocked" => Color.Error,
            "Waiting" => Color.Warning,
            _ => Color.Default,
        };

        private void CloseDialog() => MudDialog?.Cancel();

        private async Task EditProject()
        {
            var parameters = new DialogParameters<ProjectEditDialog>
            {
                { d => d.Users, Users },
                { d => d.Project, Project }
            };

            var dialog = await DialogService.ShowAsync<ProjectEditDialog>(parameters);
            var dialogResult = await dialog.Result;

            if (dialogResult is { Canceled: false, Data: ProjectDTO updatedProject })
            {
                var saved = await ArgoService.UpdateProject(updatedProject.Id, updatedProject);
                if (saved.IsSuccess)
                {
                    Snackbar.Add("Project saved", Severity.Success);
                    Project = updatedProject;
                    StateHasChanged();
                }
                else
                {
                    Snackbar.Add(string.Join("; ", saved.Errors.Select(e => e.Message)), Severity.Error);
                }
            }
        }*/

}
