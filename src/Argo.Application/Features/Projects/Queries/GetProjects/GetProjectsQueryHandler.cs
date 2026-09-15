using Argo.Application.Common.Mapping;
using Argo.Application.Common.Messaging;
using Argo.Application.Repositories;
using FluentResults;
using FluentResults.Extensions;

namespace Argo.Application.Features.Projects.Queries.GetProjects;

internal class GetProjectsQueryHandler(IProjectRepository projectRepository, IMapper mapper) : IRequestHandler<GetProjectsQuery, Result<IEnumerable<ProjectDto>>>
{
    public async Task<Result<IEnumerable<ProjectDto>>> HandleAsync(GetProjectsQuery request, CancellationToken cancellationToken = default) =>
        await projectRepository.GetAllWithDetailsAsync(cancellationToken)
            .Map(mapper.Map<ProjectDto>);
}
