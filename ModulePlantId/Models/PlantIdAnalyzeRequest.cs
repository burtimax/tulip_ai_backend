using System.Text.Json.Serialization;

namespace ModulePlantId.Models;

public class PlantIdAnalyzeRequest
{
    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = [];

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    /// <summary>
    /// all / only / auto (по документации Plant.id).
    /// </summary>
    [JsonPropertyName("health")]
    public string? Health { get; set; } = "all";

    [JsonPropertyName("similar_images")]
    public bool? SimilarImages { get; set; } = true;

    [JsonPropertyName("custom_id")]
    public int? CustomId { get; set; }

    /// <summary>
    /// ISO-8601 дата/время снимка, влияет на качество распознавания.
    /// </summary>
    [JsonPropertyName("datetime")]
    public string? Datetime { get; set; }

    /// <summary>
    /// all / general (по документации Plant.id).
    /// </summary>
    [JsonPropertyName("disease_level")]
    public string? DiseaseLevel { get; set; }

    [JsonPropertyName("suggestion_filter")]
    public PlantIdSuggestionFilter? SuggestionFilter { get; set; }

    /// <summary>
    /// species / all / genus.
    /// </summary>
    [JsonPropertyName("classification_level")]
    public string? ClassificationLevel { get; set; }

    [JsonPropertyName("classification_raw")]
    public bool? ClassificationRaw { get; set; }

    [JsonPropertyName("symptoms")]
    public bool? Symptoms { get; set; }
}
