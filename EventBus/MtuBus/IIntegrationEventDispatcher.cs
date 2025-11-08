namespace Identity.Infrastructure.MtuBus;

public interface IIntegrationEventDispatcher
{
    Task PublishAsync<T>(string queueName, T message);
}