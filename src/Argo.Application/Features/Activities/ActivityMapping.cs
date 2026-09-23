using Argo.Application.Common.Mapping;
using Argo.Domain.Entities;

namespace Argo.Application.Features.Activities;

internal class ActivityMapping : IMapModule
{
    public void RegisterMaps(IMapper mapper)
    {
        mapper.Register<Activity, ActivityDto>(a => new ActivityDto(
            ActivityId: a.Id.ToString(),
            Title: a.Title,
            Owner: a.Owner,
            Status: a.Status,
            DueDate: a.DueDate,
            Notes: a.Notes
            ));
    }
}
