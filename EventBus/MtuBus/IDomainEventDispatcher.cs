using MediatR;

namespace Identity.Infrastructure.MtuBus;

public interface IDomainEventDispatcher
{
    Task PublishAsync(IEnumerable<INotification> notifications);
    Task PublishAsync(INotification notification);
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request);
}