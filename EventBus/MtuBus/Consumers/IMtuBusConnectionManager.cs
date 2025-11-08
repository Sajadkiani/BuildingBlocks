using RabbitMQ.Client;

namespace Identity.Infrastructure.MtuBus.Consumers;

public interface IMtuBusConnectionManager
{
    Task<IConnection> GetConnectionAsync();
}