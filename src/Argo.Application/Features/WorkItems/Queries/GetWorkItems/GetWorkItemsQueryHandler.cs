using Argo.Application.Common.Mapping;
using Argo.Application.Common.Messaging;
using Argo.Application.Repositories;
using Argo.Domain.ValueObjects;
using FluentResults;
using FluentResults.Extensions;

namespace Argo.Application.Features.WorkItems.Queries.GetWorkItems;

internal class GetWorkItemsQueryHandler(IWorkItemRepository workItemRepository, IMapper mapper) : IRequestHandler<GetWorkItemsQuery, Result<IEnumerable<WorkItemDto>>>
{
    public async Task<Result<IEnumerable<WorkItemDto>>> HandleAsync(GetWorkItemsQuery request, CancellationToken cancellationToken = default) =>
        await workItemRepository.GetByProjectIdAsync(ProjectId.FromTrustedValue(request.ProjectId), cancellationToken)
            .Map(mapper.Map<WorkItemDto>);
}