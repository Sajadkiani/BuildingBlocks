using EventBus.MtuBus.Consumers;
using EventBus.MtuBus.Options;
using EventBus.MtuBus.Tests;
using EventBus.Services;
using Identity.Infrastructure.MtuBus;
using Identity.Infrastructure.MtuBus.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EventBus.MtuBus.Extensions;

public static class MtuEventBusExtension
{
    public static IServiceCollection AddMtuBus(this IServiceCollection services, IConfiguration configuration, string sectionName = "Rabbitmq")
    {
        services.Configure<MtuRabbitMqOptions>(configuration.GetSection(sectionName));


        services.AddSingleton<IMtuBusConnectionManager, MtuBusConnectionManager>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IIntegrationEventLogService, IntegrationEventLogService>();
        services.AddSingleton<IIntegrationEventDispatcher, IntegrationEventDispatcher>(opt =>
        {
            var logger = opt.GetRequiredService<ILogger<IntegrationEventDispatcher>>();
            var options = opt.GetRequiredService<IOptions<MtuRabbitMqOptions>>();

            return IntegrationEventDispatcher.CreateAsync(options, logger).GetAwaiter().GetResult();
        });

        services.AddScoped<IMtuConsumer, TestConsumer>();

        services.AddHostedService<MtuBusHostedService>();

        return services;
    }
}