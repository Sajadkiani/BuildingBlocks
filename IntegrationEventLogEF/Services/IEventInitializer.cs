using System.Dynamic;
using System.Net.Mime;

namespace IntegrationEventLogEF.Services;

public interface IEventInitializer
{
    dynamic AddExtraInfo(object source);
}

public class EventInitializer : IEventInitializer
{
    public dynamic AddExtraInfo(object source)
    {
        var result = new ExpandoObject();
        IDictionary<string, object> dictionary = result;
        foreach (var property in source
                     .GetType()
                     .GetProperties()
                     .Where(p => p.CanRead && p.GetMethod.IsPublic))
        {
            dictionary[property.Name] = property.GetValue(source, null);
        }

        dictionary["Id"] = Guid.NewGuid();
        dictionary["CreationTime"] = DateTime.Now;
        dictionary["FullName"] = source!.GetType()!.Name;
        return result;
    }
}