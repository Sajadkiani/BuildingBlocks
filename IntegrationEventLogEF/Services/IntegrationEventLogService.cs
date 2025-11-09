using Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IntegrationEventLogEF.Services;

public class IntegrationEventLogService : IIntegrationEventLogService, IDisposable
{
    private readonly ILogger<IntegrationEventLogService> _logger;
    private readonly EventLogDbContext _dbContext;
    private readonly List<Type> eventTypes;
    private volatile bool disposedValue;

    public IntegrationEventLogService(
        ILogger<IntegrationEventLogService> logger,
        EventLogDbContext eventLogDbContext)
    {
        _logger = logger;
        _dbContext = eventLogDbContext;
    }
    
    public async Task PublishEventsThroughEventBusAsync(Guid transactionId)
    {
        var pendingLogEvents = await RetrieveEventLogsPendingToPublishAsync(transactionId);
        foreach (var logEvt in pendingLogEvents)
        {
            _logger.LogInformation($"publishing event: ({JsonConvert.SerializeObject(logEvt)})");

            var eventType = eventTypes.FirstOrDefault(item => item.Name == logEvt.EventTypeName);
            if (eventType is null)
            {
                throw new Exception("event ");
            }
            
            try
            {
                await MarkEventAsInProgressAsync(logEvt.EventId);
                //await eventBus.Publish(deserializedEvent);
                await MarkEventAsPublishedAsync(logEvt.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"error publishing event: {JsonConvert.SerializeObject(logEvt)}");
                await MarkEventAsFailedAsync(logEvt.EventId);
            }
        }
    }

    public async Task AddAndSaveEventAsync<TEvent>(TEvent evt)
    {
        _logger.LogInformation($"enqueuing event: {JsonConvert.SerializeObject(evt)}");

        await SaveEventAsync(evt, _dbContext.GetCurrentTransaction());
    }

    public async Task<IEnumerable<AppEventLog>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId)
    {
        var tid = transactionId.ToString();

        return await _dbContext.EventLogs
            .Where(e => e.TransactionId == tid && e.State == EventStateEnum.NotPublished).ToListAsync();
    }

    public Task SaveEventAsync<TEvent>(TEvent @event, IDbContextTransaction transaction)
    {
        if (transaction == null || @event is null) 
            throw new ArgumentNullException(nameof(transaction));

        var eventLogEntry = new AppEventLog(@event, transaction.TransactionId, @event.GetType());

        _dbContext.Database.UseTransaction(transaction.GetDbTransaction());
        _dbContext.EventLogs.Add(eventLogEntry);
        return _dbContext.SaveChangesAsync();
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
        var eventLogEntry = _dbContext.EventLogs.Single(ie => ie.EventId == eventId);
        eventLogEntry.State = status;

        if (status == EventStateEnum.InProgress)
            eventLogEntry.TimesSent++;

        _dbContext.EventLogs.Update(eventLogEntry);

        return _dbContext.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _dbContext?.Dispose();
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
