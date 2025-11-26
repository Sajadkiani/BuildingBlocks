using System.Text.Json.Serialization;

namespace AppEvents;

public class IntegrationEvent
{        
    public IntegrationEvent()
    {
        EventId = Guid.NewGuid();
        CreationDate = DateTime.Now;
    }

    [JsonInclude]
    public Guid EventId { get; private init; }

    [JsonInclude]
    public DateTime CreationDate { get; private init; }
    
    [JsonInclude]
    public string FullName { get; private init; }
}