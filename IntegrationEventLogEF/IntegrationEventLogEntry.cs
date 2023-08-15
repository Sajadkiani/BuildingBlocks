using Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF;

namespace IntegrationEventLogEF;

public class IntegrationEventLogEntry
{
    private IntegrationEventLogEntry() { }
    public IntegrationEventLogEntry(dynamic @event, Guid transactionId, Type type, EventEnvironmentType eventEnvironmentType)
    {
        EventEnvironmentType = eventEnvironmentType;
        EventId = (Guid)@event.Id;
        CreationTime = @event.CreationDate;
        EventTypeName = type.Name;
        Content = JsonSerializer.Serialize(@event, @event.GetType(), new JsonSerializerOptions
        {
            WriteIndented = true
        });
        State = EventStateEnum.NotPublished;
        TimesSent = 0;
        TransactionId = transactionId.ToString();

    }
    public Guid EventId { get; private set; }
    public string EventTypeName { get; private set; }
    [NotMapped]
    public string EventTypeShortName => EventTypeName.Split('.')?.Last();
    [NotMapped]
    public EventStateEnum State { get; set; }
    public int TimesSent { get; set; }
    public DateTime CreationTime { get; private set; }
    public string Content { get; private set; }
    public string TransactionId { get; private set; }
    public EventEnvironmentType EventEnvironmentType { get; private set; }

    public TEvent DeserializeJsonContent<TEvent>(Type type) where TEvent : class
    {
        return (JsonSerializer.Deserialize(Content, type, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) as TEvent)!;
    }
}
