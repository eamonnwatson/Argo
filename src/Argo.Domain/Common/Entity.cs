using Argo.Domain.Interfaces;

namespace Argo.Domain.Common;

public abstract class Entity<TId> : IHasDomainEvents where TId : notnull
{
    private readonly List<IDomainEvent> domainEvents = [];

    public TId Id { get; protected init; } = default!;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents;

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => (GetType().ToString() + Id).GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}
