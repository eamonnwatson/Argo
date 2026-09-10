using Argo.Domain.Interfaces;
using Argo.Domain.ValueObjects;

namespace Argo.Domain.Events;

public sealed record ProjectManagerChanged(ProjectId ProjectId, UserId? PreviousOwnerId, UserId? NewOwnerId) : IDomainEvent;