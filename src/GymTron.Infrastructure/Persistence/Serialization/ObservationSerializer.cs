using System.Text.Json;
using GymTron.Domain.ValueObjects;

namespace GymTron.Infrastructure.Persistence.Serialization;

internal static class ObservationSerializer
{
    public static string Serialize(IEnumerable<Observation> observations)
    {
        List<string> comments = observations
            .Where(o => !string.IsNullOrWhiteSpace(o.Comment))
            .Select(o => o.Comment)
            .ToList();

        return JsonSerializer.Serialize(comments);
    }

    public static List<string> DeserializeComments(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        string trimmed = raw.Trim();
        if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
        {
            try
            {
                List<string>? comments = JsonSerializer.Deserialize<List<string>>(trimmed);
                if (comments != null)
                {
                    return comments
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Select(c => c.Trim())
                        .ToList();
                }
            }
            catch (JsonException)
            {
                // Fall back to legacy semicolon-delimited comments when JSON parsing fails.
            }
        }

        return raw.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .ToList();
    }

    public static List<Observation> Deserialize(string? raw)
        => DeserializeComments(raw).Select(c => new Observation(c)).ToList();
}
