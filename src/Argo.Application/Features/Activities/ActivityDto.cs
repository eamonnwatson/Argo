using Argo.Domain.Enums;

namespace Argo.Application.Features.Activities;

public record ActivityDto(string ActivityId, string Title, string Owner, ActivityStatus Status, DateOnly DueDate, string Notes);
