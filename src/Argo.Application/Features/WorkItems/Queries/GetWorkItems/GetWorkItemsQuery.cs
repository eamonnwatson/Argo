using Argo.Application.Common.Messaging;
using FluentResults;

namespace Argo.Application.Features.WorkItems.Queries.GetWorkItems;

public record GetWorkItemsQuery(string ProjectId) : IRequest<Result<IEnumerable<WorkItemDto>>>;
