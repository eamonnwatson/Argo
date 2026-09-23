using Argo.Application.Common.Messaging;
using FluentResults;

namespace Argo.Application.Features.RaidItems.Queries.GetRaidItems;

public record GetRaidItemsQuery(string ProjectId) : IRequest<Result<IEnumerable<RaidItemDto>>>;
