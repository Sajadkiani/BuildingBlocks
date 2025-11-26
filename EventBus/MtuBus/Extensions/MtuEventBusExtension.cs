using EventBus.MtuBus.Consumers;
using EventBus.MtuBus.Options;
using EventBus.MtuBus.Tests;
using EventBus.Services;
using Identity.Infrastructure.MtuBus;
using Identity.Infrastructure.MtuBus.Consumers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EventBus.MtuBus.Extensions;

public static class MtuEventBusExtension
{
    public static IServiceCollection AddMtuBus(this IServiceCollection services, IConfiguration configuration,
        string connectionStringSectionName, string sectionName = "Rabbitmq")
    {
        var connectionString = configuration.GetConnectionString(connectionStringSectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("connection string not found");
        }

        var mtuRabbitmq = configuration.GetSection(sectionName);
        if (string.IsNullOrWhiteSpace(connectionStringSectionName))
        {
            throw new Exception("rabbitmq section not found");
        }
        
        services.Configure<MtuRabbitMqOptions>(mtuRabbitmq);
        AddMtuPublisher(services, connectionString);
        AddMtuConsumer(services);

        return services;
    }

    private static void AddMtuConsumer(IServiceCollection services)
    {
        services.AddScoped<IMtuConsumer, TestConsumer>();

        services.AddHostedService<MtuBusHostedService>();
    }

    private static void AddMtuPublisher(IServiceCollection services, string dbConnectionString)
    {
        services.AddDbContext<EventDbContext>(opt => opt.UseSqlServer(dbConnectionString));
        services.AddSingleton<IMtuBusConnectionManager, MtuBusConnectionManager>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IIntegrationEventLogService, IntegrationEventLogService>();
        services.AddSingleton<IIntegrationEventDispatcher, IntegrationEventDispatcher>(opt =>
        {
            var logger = opt.GetRequiredService<ILogger<IntegrationEventDispatcher>>();
            var options = opt.GetRequiredService<IOptions<MtuRabbitMqOptions>>();

            return IntegrationEventDispatcher.CreateAsync(options, logger).GetAwaiter().GetResult();
        });
    }
}