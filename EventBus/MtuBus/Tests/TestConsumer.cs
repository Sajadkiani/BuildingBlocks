using AppEvents;
using EventBus;
using EventBus.MtuBus.Tests;
using Events;
using Identity.Infrastructure.MtuBus;
using Microsoft.Extensions.Logging;

public class TestConsumer : MtuConsumer
{
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ILogger<TestConsumer> _logger;
    
    public TestConsumer(
        IDomainEventDispatcher eventDispatcher,
        EventDbContext context,
        ILogger<TestConsumer> logger) 
        : base(context)
    {
        _eventDispatcher = eventDispatcher;
        _logger = logger;
        QueueName = nameof(TestIntegrationEvent);
    }

    protected override async Task HandleEventAsync(IntegratedEvent message, CancellationToken cancellationToken)
    {
        if (message is not TestIntegrationEvent testEvent)
        {
            _logger.LogError($"Received wrong event type for queue {QueueName}. EventId : {message.EventId} fullname: {message.FullName}");
            return;
        }

        _logger.LogInformation($"Received message id {message.EventId}, name {message.FullName} queue {QueueName}");

        await _eventDispatcher.PublishAsync(new TestDomainEvent(testEvent.UserName));
    }
}