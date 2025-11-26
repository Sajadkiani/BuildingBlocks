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

    public string QueueName { get; set; } = nameof(TestIntegrationEvent);

    public TestConsumer(
        IDomainEventDispatcher eventDispatcher,
        EventDbContext context,
        ILogger<TestConsumer> logger) 
        : base(context)
    {
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    protected override async Task HandleEventAsync(IntegratedEvent message, CancellationToken cancellationToken)
    {
        if (message is not TestIntegrationEvent testEvent)
        {
            _logger.LogWarning($"Received wrong event type for {QueueName}. Event was: {message.FullName}");
            return;
        }

        _logger.LogInformation($"Processing message for {QueueName}");

        await _eventDispatcher.PublishAsync(new TestDomainEvent(testEvent.UserName));
    }
}