using GymTron.Domain.ValueObjects;
using GymTron.Infrastructure.Persistence.Serialization;

namespace GymTron.UnitTests.Infrastructure;

public class ObservationSerializerTests
{
    [Fact]
    public void Serialize_WithObservations_ReturnsJsonArray()
    {
        List<Observation> observations =
        [
            new("First comment; with semicolon"),
            new("Second comment")
        ];

        string json = ObservationSerializer.Serialize(observations);

        Assert.Equal("[\"First comment; with semicolon\",\"Second comment\"]", json);
    }

    [Fact]
    public void Serialize_WithEmptyOrWhitespace_FiltersEmptyItems()
    {
        List<Observation> observations =
        [
            new("   "),
            new("Valid")
        ];

        string json = ObservationSerializer.Serialize(observations);

        Assert.Equal("[\"Valid\"]", json);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deserialize_WithNullOrWhitespace_ReturnsEmptyList(string? raw)
    {
        List<Observation> result = ObservationSerializer.Deserialize(raw);
        List<string> comments = ObservationSerializer.DeserializeComments(raw);

        Assert.Empty(result);
        Assert.Empty(comments);
    }

    [Fact]
    public void Deserialize_WithValidJson_ReturnsParsedObservations()
    {
        string json = "[\"First comment; with semicolon\", \"Second comment\"]";

        List<Observation> result = ObservationSerializer.Deserialize(json);
        List<string> comments = ObservationSerializer.DeserializeComments(json);

        Assert.Equal(2, result.Count);
        Assert.Equal("First comment; with semicolon", result[0].Comment);
        Assert.Equal("Second comment", result[1].Comment);
        Assert.Equal(["First comment; with semicolon", "Second comment"], comments);
    }

    [Fact]
    public void Deserialize_WithLegacySemicolonString_ReturnsParsedObservations()
    {
        string legacy = "steady;clean; tempo ";

        List<Observation> result = ObservationSerializer.Deserialize(legacy);
        List<string> comments = ObservationSerializer.DeserializeComments(legacy);

        Assert.Equal(3, result.Count);
        Assert.Equal(["steady", "clean", "tempo"], result.Select(o => o.Comment));
        Assert.Equal(["steady", "clean", "tempo"], comments);
    }

    [Fact]
    public void Deserialize_WithMalformedJsonStartingWithBracket_FallsBackToSemicolon()
    {
        string malformed = "[broken json; not valid";

        List<Observation> result = ObservationSerializer.Deserialize(legacyOrMalformed(malformed));
        Assert.NotEmpty(result);
    }

    private static string legacyOrMalformed(string s) => s;
}
