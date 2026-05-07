namespace CustomerApi.Core.SharedKernel;

public abstract class BaseEntity : IEntity<Guid>
{
    private readonly List<BaseEvent> _domainEvents = [];
    protected BaseEntity() => Id = Guid.NewGuid();
    protected BaseEntity(Guid id) => Id = id;
   
    public IEnumerable<BaseEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    public Guid Id { get; private init; }

    protected void AddDomainEvent(BaseEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() =>
        _domainEvents.Clear();
}
