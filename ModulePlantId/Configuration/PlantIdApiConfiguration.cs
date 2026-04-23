namespace ModulePlantId.Configuration;

public class PlantIdApiConfiguration
{
    public const string Section = "PlantId";

    /// <summary>
    /// Использовать mock-сервис вместо реального вызова Plant.id.
    /// </summary>
    public bool UseMockService { get; set; } = false;

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://plant.id";

    /// <summary>Относительный endpoint в API Plant.id.</summary>
    public string AnalyzeEndpoint { get; set; } = "api/v3/identification";

    public int Timeout { get; set; } = 120;

    public int MaxRetryAttempts { get; set; } = 3;

    public int RetryDelayMs { get; set; } = 1000;
}
