using Argo.Application.Common.Mapping;
using Argo.Domain.Entities;

namespace Argo.Application.Features.Users;

internal class UserMapping : IMapModule
{
    public void RegisterMaps(IMapper mapper)
    {
        mapper.Register<User, UserDto>(user => new UserDto(user.Id.ToString(), user.DisplayName, user.Email ?? string.Empty));
    }
}
