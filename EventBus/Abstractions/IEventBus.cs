using MediatR;

namespace EventBus.Abstractions;

public interface IEventBus
{
    Task<TResponse> SendMediator<TResponse>(IRequest<TResponse> command);
    Task PublishMediator<TNotification>(TNotification notification);
    Task Publish<TIntegrationEvent>(TIntegrationEvent @event);
}