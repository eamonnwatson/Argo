using Argo.Application.Common.Messaging;
using FluentResults;

namespace Argo.Application.Features.Users.Queries.GetProjectManagers;

public record GetProjectManagersQuery : IRequest<Result<IEnumerable<UserDto>>>;
