using Argo.Application.Common.Messaging;
using FluentResults;

namespace Argo.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<Result<IEnumerable<UserDto>>>;