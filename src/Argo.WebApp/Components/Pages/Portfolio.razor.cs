using Argo.Application.Common.Messaging;
using Argo.Application.Features.Projects;
using Argo.Application.Features.Projects.Queries.GetProjects;
using Argo.Application.Features.Users;
using Argo.Application.Features.Users.Queries.GetProjectManagers;
using Argo.WebApp.Infrastructure;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Argo.WebApp.Components.Pages;

public partial class Portfolio
{
    [Inject] private IDispatcher Dispatcher { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private bool loading = true;

    private List<ProjectDto> projects = [];
    private List<UserDto> users = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        loading = true;

        var projectsResult = await Dispatcher.SendAsync(new GetProjectsQuery());

        if (projectsResult.ShowErrorsIfFailed(Snackbar))
            return;

        projects = projectsResult.Value.ToList();

        var usersResult = await Dispatcher.SendAsync(new GetProjectManagersQuery());

        if (usersResult.ShowErrorsIfFailed(Snackbar))
            return;

        users = usersResult.Value.ToList();

        loading = false;
    }




    private async Task OpenAddProjectDialog()
    {
        //var parameters = new DialogParameters<ProjectEditDialog>
        //{
        //    { d => d.Users, users }
        //};

        //var dialog = await dialogService.ShowAsync<ProjectEditDialog>(parameters);
        //var dialogResult = await dialog.Result;

        //if (dialogResult is { Canceled: false, Data: ProjectCreateDTO createDto })
        //{
        //    var created = await argoService.CreateProject(createDto);
        //if (created.IsSuccess)
        //{
        //snackbar.Add("Project created", Severity.Success);
        //await LoadDataAsync();
        //}
        //else
        //{
        //snackbar.Add(string.Join("; ", created.Errors.Select(e => e.Message)), Severity.Error);
        //}
        //}

    }




    //private async Task SelectProject(string projectId)
    //{
    //    if (selectedProjectId == projectId)
    //    {
    //        selectedProjectId = null;
    //        if (portfolioDetailDialog is not null)
    //        {
    //            portfolioDetailDialog.Close();
    //            portfolioDetailDialog = null;
    //        }
    //        return;
    //    }

    //    selectedProjectId = projectId;
    //    await OpenPortfolioDetailDialogAsync();
    //}

    //private async Task OpenPortfolioDetailDialogAsync()
    //{
    //    if (SelectedProject is null)
    //    {
    //        return;
    //    }

    //    var parameters = new DialogParameters<PortfolioDetail>
    //    {
    //        { d => d.Project, SelectedProject },
    //        { d => d.WorkItems, WorkItemsForSelectedProject },
    //        { d => d.Activities, ActivitiesForSelectedProject },
    //        { d => d.RaidItems, RaidItemsForSelectedProject },
    //        { d => d.Users, users },
    //        { d => d.OnAddWorkItem, EventCallback.Factory.Create(this, () => OpenWorkItemDialog(null)) },
    //        { d => d.OnEditWorkItem, EventCallback.Factory.Create<WorkItemDTO>(this, workItem => OpenWorkItemDialog(workItem)) },
    //        { d => d.OnEditProject, EventCallback.Factory.Create<ProjectDTO>(this, OpenEditProjectDialog) },
    //        { d => d.OnDeleteProject, EventCallback.Factory.Create<ProjectDTO>(this, DeleteProjectAsync) },
    //        { d => d.OnAddRaidItem, EventCallback.Factory.Create(this, ShowRaidComingSoon) }
    //    };

    //    var options = new DialogOptions
    //    {
    //        FullWidth = false,
    //        MaxWidth = MaxWidth.False,
    //        NoHeader = true,
    //        CloseButton = false
    //    };

    //    portfolioDetailDialog = await DialogService.ShowAsync<PortfolioDetail>(SelectedProject.Name, parameters, options);
    //    _ = await portfolioDetailDialog.Result;

    //    selectedProjectId = null;
    //    portfolioDetailDialog = null;
    //}

    //private void ShowRaidComingSoon()
    //{
    //    Snackbar.Add("RAID record management is coming soon", Severity.Info);
    //}

    //private async Task OpenAddProjectDialog()
    //{
    //    var parameters = new DialogParameters<ProjectDialog>
    //    {
    //        { d => d.Users, users }
    //    };
    //    var dialog = await DialogService.ShowAsync<ProjectDialog>("Add project", parameters);
    //    var result = await dialog.Result;
    //    if (result is { Canceled: false, Data: ProjectCreateDTO createDto })
    //    {
    //        var created = await ArgoService.CreateProject(createDto);
    //        if (created.IsSuccess)
    //        {
    //            Snackbar.Add("Project created", Severity.Success);
    //            await LoadDataAsync();
    //        }
    //        else
    //        {
    //            Snackbar.Add(string.Join("; ", created.Errors.Select(e => e.Message)), Severity.Error);
    //        }
    //    }
    //}

    //private async Task OpenEditProjectDialog(ProjectDTO project)
    //{
    //    var parameters = new DialogParameters<ProjectDialog>
    //    {
    //        { d => d.Users, users },
    //        { d => d.Existing, project }
    //    };

    //    var dialogOptions = new DialogOptions()
    //    {
    //        FullWidth = true,
    //        MaxWidth = MaxWidth.Medium
    //    };

    //    var dialog = await DialogService.ShowAsync<ProjectDialog>("Edit project", parameters, dialogOptions);
    //    var result = await dialog.Result;
    //    if (result is { Canceled: false, Data: ProjectDTO updated })
    //    {
    //        var saved = await ArgoService.UpdateProject(project.Id, updated);
    //        if (saved.IsSuccess)
    //        {

    //            Snackbar.Add("Project saved", Severity.Success);
    //            await LoadDataAsync();
    //        }
    //        else
    //        {
    //            Snackbar.Add(string.Join("; ", saved.Errors.Select(e => e.Message)), Severity.Error);
    //        }
    //    }
    //}

    //private async Task DeleteProjectAsync(ProjectDTO project)
    //{
    //    var confirmed = await mudDeleteBox.ShowAsync(new DialogOptions() { Position = DialogPosition.TopCenter });

    //    if (confirmed != true) return;

    //    var deleted = await ArgoService.DeleteProject(project.Id);
    //    if (deleted.IsSuccess)
    //    {
    //        Snackbar.Add("Project deleted", Severity.Success);
    //        if (selectedProjectId == project.Id) selectedProjectId = null;
    //        await LoadDataAsync();
    //    }
    //    else
    //    {
    //        Snackbar.Add(string.Join("; ", deleted.Errors.Select(e => e.Message)), Severity.Error);
    //    }
    //}

    //private async Task OpenWorkItemDialog(WorkItemDTO? existing)
    //{
    //    if (SelectedProject is null) return;

    //    var parameters = new DialogParameters<WorkItemDialog>
    //    {
    //        { d => d.Users, users },
    //        { d => d.ProjectId, SelectedProject.Id },
    //        { d => d.Existing, existing }
    //    };
    //    var dialog = await DialogService.ShowAsync<WorkItemDialog>(existing is null ? "Work package or wave" : "Edit work item", parameters);
    //    var result = await dialog.Result;
    //    if (result is { Canceled: false })
    //    {
    //        if (result.Data is WorkItemCreateDTO createDto)
    //        {
    //            var created = await ArgoService.CreateWorkItem(createDto);
    //            if (created.IsFailed)
    //            {
    //                Snackbar.Add(string.Join("; ", created.Errors.Select(e => e.Message)), Severity.Error);
    //                return;
    //            }
    //        }
    //        else if (result.Data is WorkItemDTO updateDto)
    //        {
    //            var updated = await ArgoService.UpdateWorkItem(updateDto.Id, updateDto);
    //            if (updated.IsFailed)
    //            {
    //                Snackbar.Add(string.Join("; ", updated.Errors.Select(e => e.Message)), Severity.Error);
    //                return;
    //            }
    //        }

    //        Snackbar.Add("Work item saved", Severity.Success);
    //        await LoadDataAsync();
    //    }
    //}

}
