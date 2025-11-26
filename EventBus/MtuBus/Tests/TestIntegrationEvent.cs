using AppEvents;

namespace Events;

public class TestIntegrationEvent : IntegratedEvent
{
    public string UserName { get; init; }
}
    