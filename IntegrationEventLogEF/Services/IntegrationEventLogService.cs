using Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF;

namespace IntegrationEventLogEF.Services;

public class IntegrationEventLogService : IIntegrationEventLogService, IDisposable
{
    private readonly IntegrationEventLogContext integrationEventLogContext;
    private readonly DbConnection dbConnection;
    private readonly List<Type> eventTypes;
    private volatile bool disposedValue;

    public IntegrationEventLogService(DbConnection dbConnection)
    {
        dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        integrationEventLogContext = new IntegrationEventLogContext(
            new DbContextOptionsBuilder<IntegrationEventLogContext>()
                .UseSqlServer(dbConnection)
                .Options);
    }

    public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId)
    {
        var tid = transactionId.ToString();

        return await integrationEventLogContext.IntegrationEventLogs
            .Where(e => e.TransactionId == tid && e.State == EventStateEnum.NotPublished).ToListAsync();
    }

    public Task SaveEventAsync<TEvent>(TEvent @event, IDbContextTransaction transaction)
    {
        if (transaction == null || @event is null) throw new ArgumentNullException(nameof(transaction));

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var eventEnvironmentType = EventEnvironmentType.Production;
        if (string.Equals(env, "Development", StringComparison.Ordinal))
            eventEnvironmentType = EventEnvironmentType.Development;

        //TODO: get event environment type form outside
        var eventLogEntry = new IntegrationEventLogEntry(@event, transaction.TransactionId, @event.GetType(), eventEnvironmentType);

        integrationEventLogContext.Database.UseTransaction(transaction.GetDbTransaction());
        integrationEventLogContext.IntegrationEventLogs.Add(eventLogEntry);

        return integrationEventLogContext.SaveChangesAsync();
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
        var eventLogEntry = integrationEventLogContext.IntegrationEventLogs.Single(ie => ie.EventId == eventId);
        eventLogEntry.State = status;

        if (status == EventStateEnum.InProgress)
            eventLogEntry.TimesSent++;

        integrationEventLogContext.IntegrationEventLogs.Update(eventLogEntry);

        return integrationEventLogContext.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                integrationEventLogContext?.Dispose();
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
