namespace CustomerApi.Core.SharedKernel;

public class EventStore : BaseEvent
{
    public EventStore(Guid aggregateId, string messageType, string data)
    {
        AggregateId = aggregateId;
        MessageType = messageType;
        Data = data;
    }

    public EventStore()
    {
    }
    public Guid Id { get; private init; } = Guid.NewGuid();
    public string Data { get; private init; }
}
