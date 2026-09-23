using Argo.Application.Common.Mapping;
using Argo.Application.Common.Messaging;
using Argo.Application.Repositories;
using Argo.Domain.ValueObjects;
using FluentResults;
using FluentResults.Extensions;

namespace Argo.Application.Features.RaidItems.Queries.GetRaidItems;

internal class GetRaidItemsQueryHandler(IRaidItemRepository raidItemRepository, IMapper mapper) : IRequestHandler<GetRaidItemsQuery, Result<IEnumerable<RaidItemDto>>>
{
    public async Task<Result<IEnumerable<RaidItemDto>>> HandleAsync(GetRaidItemsQuery request, CancellationToken cancellationToken = default) =>
        await raidItemRepository.GetByProjectIdAsync(ProjectId.FromTrustedValue(request.ProjectId), cancellationToken)
            .Map(mapper.Map<RaidItemDto>);
}
