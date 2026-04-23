using System.Text.Json.Serialization;

namespace ModulePlantId.Models;

public class PlantIdRequestOptions
{
    /// <summary>
    /// comma-separated list of requested details.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// one or more language codes, separated by commas (e.g. en,de).
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// If true, response can be returned without result block.
    /// </summary>
    public bool? Async { get; set; }
}

public class PlantIdSuggestionFilter
{
    [JsonPropertyName("classification")]
    public string? Classification { get; set; }
}
