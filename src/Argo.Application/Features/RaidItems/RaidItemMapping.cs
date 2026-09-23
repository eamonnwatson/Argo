using Argo.Application.Common.Mapping;
using Argo.Domain.Entities;

namespace Argo.Application.Features.RaidItems;

internal class RaidItemMapping : IMapModule
{
    public void RegisterMaps(IMapper mapper)
    {
        mapper.Register<RaidItem, RaidItemDto>(ri => new RaidItemDto(
            RaidItemId: ri.Id.ToString(),
            Type: ri.Type,
            Description: ri.Description,
            Owner: ri.Owner,
            DueDate: ri.DueDate
            ));
    }
}
