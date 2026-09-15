using Argo.Application.Common.Messaging;
using FluentResults;

namespace Argo.Application.Features.Projects.Queries.GetProjects;

public record GetProjectsQuery : IRequest<Result<IEnumerable<ProjectDto>>>;
