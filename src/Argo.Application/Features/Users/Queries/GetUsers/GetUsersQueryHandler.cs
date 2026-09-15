using Argo.Application.Common.Mapping;
using Argo.Application.Common.Messaging;
using Argo.Application.Repositories;
using FluentResults;
using FluentResults.Extensions;

namespace Argo.Application.Features.Users.Queries.GetUsers;

internal class GetUsersQueryHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetUsersQuery, Result<IEnumerable<UserDto>>>
{
    public async Task<Result<IEnumerable<UserDto>>> HandleAsync(GetUsersQuery request, CancellationToken cancellationToken = default) =>
        await userRepository.GetAllAsync(cancellationToken)
            .Map(mapper.Map<UserDto>);
}
