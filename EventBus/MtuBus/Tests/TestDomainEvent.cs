using MediatR;

namespace EventBus.MtuBus.Tests;

public class TestDomainEvent : INotification
{
    public string UserName { get; }

    public TestDomainEvent(string userName)
    {
        UserName = userName;
    }
}