namespace EventBus.Services;

public interface IIntegrationEventLogService
{
    Task PublishEventsThroughEventBusAsync(Guid transactionId);
    Task<IEnumerable<AppEventLog>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);
    Task SaveEventAsync<TEvent>(TEvent @event, IDbContextTransaction transaction);
    Task MarkEventAsPublishedAsync(Guid eventId);
    Task MarkEventAsInProgressAsync(Guid eventId);
    Task MarkEventAsFailedAsync(Guid eventId);
}
