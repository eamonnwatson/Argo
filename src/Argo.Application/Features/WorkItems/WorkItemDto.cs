using Argo.Application.Features.Activities;
using Argo.Domain.Enums;

namespace Argo.Application.Features.WorkItems;

public record WorkItemDto(string WorkItemId, string Title, string Owner, WorkItemStatus Status, DateOnly DueDate, string Dependency, string Purpose, string Participants, string RequiredInputs, string Milestone, string DefinitionOfDone, IEnumerable<ActivityDto> Activities);
