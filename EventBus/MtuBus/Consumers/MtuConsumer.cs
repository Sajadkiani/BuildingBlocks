using AppEvents;
using EventBus;
using Newtonsoft.Json;

public abstract class MtuConsumer
{
    private readonly EventDbContext _context;
    public string QueueName { get; set; }

    protected MtuConsumer(EventDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(string json, CancellationToken cancellationToken)
    {
        if (json == null)
            throw new NullReferenceException("json message is null");

        var message = JsonConvert.DeserializeObject<IntegratedEvent>(json);
        if (message == null)
            throw new NullReferenceException("message is null");

        await _context.AppReceivedEvents.AddAsync(new AppReceivedEvent(message.EventId, message.FullName, json),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        
        await HandleEventAsync(message, cancellationToken);
    }

    protected abstract Task HandleEventAsync(IntegratedEvent message, CancellationToken cancellationToken);
}