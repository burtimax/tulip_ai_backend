using System.Text.Json.Serialization;

namespace ModulePlantId.Models;

public class PlantIdAnalyzeResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("model_version")]
    public string? ModelVersion { get; set; }

    [JsonPropertyName("custom_id")]
    public string? CustomId { get; set; }

    [JsonPropertyName("input")]
    public PlantIdInputData? Input { get; set; }

    [JsonPropertyName("result")]
    public PlantIdResultData? Result { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("sla_compliant_client")]
    public bool? SlaCompliantClient { get; set; }

    [JsonPropertyName("sla_compliant_system")]
    public bool? SlaCompliantSystem { get; set; }

    [JsonPropertyName("created")]
    public double? Created { get; set; }

    [JsonPropertyName("completed")]
    public double? Completed { get; set; }

    public string RawJson { get; set; } = string.Empty;
}

public class PlantIdInputData
{
    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("health")]
    public string? Health { get; set; }

    [JsonPropertyName("similar_images")]
    public bool? SimilarImages { get; set; }

    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = [];

    [JsonPropertyName("datetime")]
    public DateTimeOffset? Datetime { get; set; }
}

public class PlantIdResultData
{
    [JsonPropertyName("disease")]
    public PlantIdDiseaseData? Disease { get; set; }

    [JsonPropertyName("classification")]
    public PlantIdClassificationData? Classification { get; set; }

    [JsonPropertyName("is_plant")]
    public PlantIdBinaryResult? IsPlant { get; set; }

    [JsonPropertyName("is_healthy")]
    public PlantIdBinaryResult? IsHealthy { get; set; }
}

public class PlantIdDiseaseData
{
    [JsonPropertyName("suggestions")]
    public List<PlantIdSuggestion> Suggestions { get; set; } = [];

    [JsonPropertyName("question")]
    public string? Question { get; set; }
}

public class PlantIdClassificationData
{
    [JsonPropertyName("suggestions")]
    public List<PlantIdSuggestion> Suggestions { get; set; } = [];
}

public class PlantIdSuggestion
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("probability")]
    public double? Probability { get; set; }

    [JsonPropertyName("similar_images")]
    public List<PlantIdSimilarImage> SimilarImages { get; set; } = [];

    [JsonPropertyName("details")]
    public PlantIdDetails? Details { get; set; }
}

public class PlantIdSimilarImage
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("license_name")]
    public string? LicenseName { get; set; }

    [JsonPropertyName("license_url")]
    public string? LicenseUrl { get; set; }

    [JsonPropertyName("citation")]
    public string? Citation { get; set; }

    [JsonPropertyName("similarity")]
    public double? Similarity { get; set; }

    [JsonPropertyName("url_small")]
    public string? UrlSmall { get; set; }
}

public class PlantIdDetails
{
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("entity_id")]
    public string? EntityId { get; set; }
}

public class PlantIdBinaryResult
{
    [JsonPropertyName("probability")]
    public double? Probability { get; set; }

    [JsonPropertyName("threshold")]
    public double? Threshold { get; set; }

    [JsonPropertyName("binary")]
    public bool? Binary { get; set; }
}
