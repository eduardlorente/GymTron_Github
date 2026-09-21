using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace GymTron.App.Helpers;

public class JsonReader
{


    public static T? ReadJson<T>(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var fullResourceName = $"{assembly.GetName().Name}.{resourceName}";

        using Stream stream = assembly.GetManifestResourceStream(fullResourceName) 
            ?? throw new Exception($"No s'ha trobat l'arxiu JSON: {resourceName}");

        using StreamReader reader = new(stream, Encoding.UTF8);

        string json = reader.ReadToEnd();
        
        return JsonConvert.DeserializeObject<T>(json);
    }
}
