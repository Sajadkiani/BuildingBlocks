using EventBus.MtuBus.Consumers;
using Events;
using Identity.Infrastructure.MtuBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventBus.MtuBus.Tests;

public class TestConsumer : IMtuConsumer
{
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ILogger<TestConsumer> _logger;
    public string QueueName { get; set; } = nameof(TestIntegrationEvent);
    public TestConsumer(
        IDomainEventDispatcher eventDispatcher,
        ILogger<TestConsumer> logger)
    {
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(string messageJson, CancellationToken cancellationToken)
    {
        var message = JsonConvert.DeserializeObject<TestIntegrationEvent>(messageJson);
        if (message == null)
        {
            _logger.LogWarning($"Received null message for {QueueName}");
            return;
        }

        _logger.LogInformation($"Processing message for {QueueName}");

        await _eventDispatcher.PublishAsync(new TestDomainEvent(message.UserName));
    }
}