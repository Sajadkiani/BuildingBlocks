using AppEvents;

namespace Events;

public class TestIntegrationEvent : IntegrationEvent
{
    public string UserName { get; init; }
}
    