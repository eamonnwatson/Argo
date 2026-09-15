using Argo.Application.Common.Mapping;
using Argo.Application.Common.Messaging;
using Argo.Application.Repositories;
using FluentResults;
using FluentResults.Extensions;


namespace Argo.Application.Features.Users.Queries.GetProjectManagers;

internal class GetProjectManagerQueryHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetProjectManagersQuery, Result<IEnumerable<UserDto>>>
{
    public async Task<Result<IEnumerable<UserDto>>> HandleAsync(GetProjectManagersQuery request, CancellationToken cancellationToken = default) =>
        await userRepository.GetAllAsync(true, cancellationToken)
            .Map(mapper.Map<UserDto>);
}
