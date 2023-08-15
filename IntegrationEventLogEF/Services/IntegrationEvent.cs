using System.Text.Json.Serialization;

namespace IntegrationEventLogEF.Services;

public class IntegrationEvent
{        
    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.Now;
    }

    [JsonConstructor]
    public IntegrationEvent(Guid id, DateTime createDate)
    {
        Id = id;
        CreationDate = createDate;
        FullName = nameof(IntegrationEvent);
    }

    [JsonInclude]
    public Guid Id { get; private init; }

    [JsonInclude]
    public DateTime CreationDate { get; private init; }
    
    [JsonInclude]
    public string FullName { get; private init; }
}