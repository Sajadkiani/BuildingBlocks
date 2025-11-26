namespace EventBus.MtuBus.Consumers;

public interface IMtuConsumer
{
    public string QueueName { get; set; }
    Task HandleAsync(string json, CancellationToken cancellationToken);
}