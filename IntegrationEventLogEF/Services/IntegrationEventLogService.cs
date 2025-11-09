using Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF;

namespace IntegrationEventLogEF.Services;

public class IntegrationEventLogService : IIntegrationEventLogService, IDisposable
{
    private readonly EventLogDbContext _eventLogDbContext;
    private readonly List<Type> eventTypes;
    private volatile bool disposedValue;

    public IntegrationEventLogService(
        EventLogDbContext eventLogDbContext)
    {
        _eventLogDbContext = eventLogDbContext;
    }

    public async Task<IEnumerable<AppEventLog>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId)
    {
        var tid = transactionId.ToString();

        return await _eventLogDbContext.EventLogs
            .Where(e => e.TransactionId == tid && e.State == EventStateEnum.NotPublished).ToListAsync();
    }

    public Task SaveEventAsync<TEvent>(TEvent @event, IDbContextTransaction transaction)
    {
        if (transaction == null || @event is null) 
            throw new ArgumentNullException(nameof(transaction));

        var eventLogEntry = new AppEventLog(@event, transaction.TransactionId, @event.GetType());

        _eventLogDbContext.Database.UseTransaction(transaction.GetDbTransaction());
        _eventLogDbContext.EventLogs.Add(eventLogEntry);
        return _eventLogDbContext.SaveChangesAsync();
    }

    public Task MarkEventAsPublishedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.Published);
    }

    public Task MarkEventAsInProgressAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.InProgress);
    }

    public Task MarkEventAsFailedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.PublishedFailed);
    }

    private Task UpdateEventStatus(Guid eventId, EventStateEnum status)
    {
        var eventLogEntry = _eventLogDbContext.EventLogs.Single(ie => ie.EventId == eventId);
        eventLogEntry.State = status;

        if (status == EventStateEnum.InProgress)
            eventLogEntry.TimesSent++;

        _eventLogDbContext.EventLogs.Update(eventLogEntry);

        return _eventLogDbContext.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _eventLogDbContext?.Dispose();
            }


            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
