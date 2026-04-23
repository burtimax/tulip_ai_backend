using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModulePlantId.Models;

public class PlantIdEmptyResponse
{
}

public class PlantIdMessageResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class PlantIdUsageInfoResponse
{
    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("credit_limits")]
    public PlantIdCreditCounter? CreditLimits { get; set; }

    [JsonPropertyName("used")]
    public PlantIdCreditCounter? Used { get; set; }

    [JsonPropertyName("can_use_credits")]
    public PlantIdCanUseCredits? CanUseCredits { get; set; }

    [JsonPropertyName("remaining")]
    public PlantIdCreditCounter? Remaining { get; set; }
}

public class PlantIdCreditCounter
{
    [JsonPropertyName("day")]
    public int? Day { get; set; }

    [JsonPropertyName("week")]
    public int? Week { get; set; }

    [JsonPropertyName("month")]
    public int? Month { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }
}

public class PlantIdCanUseCredits
{
    [JsonPropertyName("value")]
    public bool? Value { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

public class PlantIdPlantSearchResponse
{
    [JsonPropertyName("entities")]
    public List<PlantIdPlantEntitySearchResult> Entities { get; set; } = [];

    [JsonPropertyName("entities_trimmed")]
    public bool? EntitiesTrimmed { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }
}

public class PlantIdPlantEntitySearchResult
{
    [JsonPropertyName("matched_in")]
    public string? MatchedIn { get; set; }

    [JsonPropertyName("matched_in_type")]
    public string? MatchedInType { get; set; }

    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("match_position")]
    public int? MatchPosition { get; set; }

    [JsonPropertyName("match_length")]
    public int? MatchLength { get; set; }

    [JsonPropertyName("entity_name")]
    public string? EntityName { get; set; }

    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }
}

public class PlantIdKbPlantDetailsResponse
{
    [JsonPropertyName("entity_id")]
    public string? EntityId { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class PlantIdConversationResponse
{
    [JsonPropertyName("messages")]
    public List<PlantIdConversationMessage> Messages { get; set; } = [];

    [JsonPropertyName("identification")]
    public string? Identification { get; set; }

    [JsonPropertyName("remaining_calls")]
    public int? RemainingCalls { get; set; }

    [JsonPropertyName("model_parameters")]
    public PlantIdConversationModelParameters? ModelParameters { get; set; }

    [JsonPropertyName("feedback")]
    public Dictionary<string, JsonElement>? Feedback { get; set; }
}

public class PlantIdConversationMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("created")]
    public DateTimeOffset? Created { get; set; }
}

public class PlantIdConversationModelParameters
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }
}
