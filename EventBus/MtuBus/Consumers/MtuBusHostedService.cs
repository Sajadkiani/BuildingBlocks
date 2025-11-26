using System.Text;
using AppEvents;
using Identity.Infrastructure.MtuBus.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.MtuBus.Consumers;

public class MtuBusHostedService : BackgroundService
{
    private readonly ILogger<MtuBusHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMtuBusConnectionManager _connectionManager;
    private IChannel? _channel;


    public MtuBusHostedService(
        IMtuBusConnectionManager connectionManager,
        ILogger<MtuBusHostedService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _connectionManager = connectionManager;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var connection = await _connectionManager.GetConnectionAsync();
        _channel = await connection.CreateChannelAsync(null, cancellationToken);

        using var startupScope = _serviceProvider.CreateScope();
        var consumers = startupScope.ServiceProvider.GetServices<MtuConsumer>().ToList();
        foreach (var consumer in consumers)
        {
            await _channel.QueueDeclareAsync(
                queue: consumer.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var rabbitConsumer = new AsyncEventingBasicConsumer(_channel);

            rabbitConsumer.ReceivedAsync += async (_, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    await consumer.HandleAsync(json, cancellationToken);
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing message from {consumer.QueueName}: {json}");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false, cancellationToken);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: consumer.QueueName,
                autoAck: false,
                consumer: rabbitConsumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation($"Consumer for queue {consumer.QueueName} started");
        }

        _logger.LogInformation($"MTU bus started with {consumers.Count()} consumers.");
    }
}