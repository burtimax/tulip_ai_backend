using System.Text.Json.Serialization;

namespace ModulePlantId.Models;

public class PlantIdErrorResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}
