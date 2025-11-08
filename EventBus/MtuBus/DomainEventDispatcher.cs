using Identity.Infrastructure.MtuBus;
using MediatR;

namespace EventBus.MtuBus;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task PublishAsync(IEnumerable<INotification> notifications)
    {
        foreach (var notification in notifications)
        {
            await _mediator.Publish(notification);
        }
    }

    public async Task PublishAsync(INotification notification)
    {
        await _mediator.Publish(notification);
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return _mediator.Send(request);
    }
}