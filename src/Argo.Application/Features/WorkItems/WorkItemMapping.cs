using Argo.Application.Common.Mapping;
using Argo.Application.Features.Activities;
using Argo.Domain.Entities;

namespace Argo.Application.Features.WorkItems;

internal class WorkItemMapping : IMapModule
{
    public void RegisterMaps(IMapper mapper)
    {
        mapper.Register<WorkItem, WorkItemDto>(wi => new WorkItemDto(
            WorkItemId: wi.Id.ToString(),
            Title: wi.Title,
            Owner: wi.Owner,
            Status: wi.Status,
            DueDate: wi.DueDate,
            Dependency: wi.Dependency,
            Purpose: wi.Purpose,
            Participants: wi.Participants,
            RequiredInputs: wi.RequiredInputs,
            Milestone: wi.Milestone,
            DefinitionOfDone: wi.DefinitionOfDone,
            Activities: mapper.Map<ActivityDto>(wi.Activities)
            ));
    }
}
