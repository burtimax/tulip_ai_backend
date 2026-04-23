using System.Text.Json.Serialization;

namespace ModulePlantId.Models;

public class PlantIdFeedbackRequest
{
    [JsonPropertyName("rating")]
    public int? Rating { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}

public class PlantIdConversationFeedbackRequest
{
    [JsonPropertyName("feedback")]
    public PlantIdFeedbackRequest? Feedback { get; set; }
}

public class PlantIdPlantSearchOptions
{
    public string Query { get; set; } = string.Empty;
    public int? Limit { get; set; }
    public string? Language { get; set; }
    public bool? Thumbnails { get; set; }
}

public class PlantIdConversationRequest
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("app_name")]
    public string? AppName { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }
}
