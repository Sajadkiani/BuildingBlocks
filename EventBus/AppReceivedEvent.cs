using AppDomain.SeedWork;

namespace EventBus;

public class AppReceivedEvent : Entity
{
    private AppReceivedEvent()
    {
    }
    
    public AppReceivedEvent(Guid eventId, string eventTypeName, string content)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        EventTypeName = eventTypeName;
        ReceivedOn = DateTime.Now;
        Content = content;
    }

    public void SetToProcessed()
    {
        State = ReceivedEventState.Processed;
    }
    
    public void SetFailedToProcessed()
    {
        State = ReceivedEventState.ProcessedFailed;
    }
    
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string EventTypeName { get; private set; }
    public string EventTypeShortName => EventTypeName.Split('.')?.Last();
    
    public ReceivedEventState State { get; private set; }
    public DateTime ReceivedOn { get; private set; }
    public string Content { get; private set; }
}