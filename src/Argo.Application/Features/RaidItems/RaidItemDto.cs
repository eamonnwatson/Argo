using Argo.Domain.Enums;

namespace Argo.Application.Features.RaidItems;

public record RaidItemDto(string RaidItemId, RaidItemType Type, string Description, string Owner, DateOnly DueDate);
