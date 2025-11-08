using System.Text;
using System.Text.Json;
using EventBus.MtuBus.Options;
using Identity.Infrastructure.MtuBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EventBus.MtuBus;

public sealed class IntegrationEventDispatcher : IIntegrationEventDispatcher, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<IntegrationEventDispatcher> _logger;

    public IntegrationEventDispatcher(
        IConnection connection,
        IChannel channel,
        ILogger<IntegrationEventDispatcher> logger)
    {
        _connection = connection;
        _channel = channel;
        _logger = logger;
    }

    public static async Task<IntegrationEventDispatcher> CreateAsync(
        IOptions<MtuRabbitMqOptions> options,
        ILogger<IntegrationEventDispatcher> logger)
    {
        if (options.Value is not MtuRabbitMqOptions busOptions) 
            return null;
        
        var factory = new ConnectionFactory
        {
            HostName = busOptions.HostName,
            UserName = busOptions.UserName,
            Password = busOptions.Password,
            VirtualHost = busOptions.VirtualHost,
            Port = busOptions.Port
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        logger.LogInformation($"Async RabbitMQ connection established to {busOptions.HostName}");
        
        return new IntegrationEventDispatcher(connection, channel, logger);
    }

    public async Task PublishAsync<T>(string queueName, T message)
    {
        try
        {
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties { Persistent = true };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: true,
                basicProperties: props,
                body: body);

            _logger.LogInformation($"Published message to queue {queueName}: {json}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error publishing to RabbitMQ queue {queueName}.");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
        _logger.LogInformation("RabbitMQ connection closed.");
    }
}