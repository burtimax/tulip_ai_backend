using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ModulePlantId.Configuration;
using ModulePlantId.Models;
using Shared.Configs;
using Shared.Contracts;

namespace ModulePlantId.Services;

public class PlantIdService : IPlantIdService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly PlantIdApiConfiguration _config;
    private readonly ILogger<PlantIdService> _logger;
    private readonly string _identifyEndpoint;
    private const string HealthAssessmentEndpoint = "api/v3/health_assessment";
    private const string UsageInfoEndpoint = "api/v3/usage_info";
    private const string PlantNameSearchEndpoint = "api/v3/kb/plants/name_search";

    public PlantIdService(
        IHttpClientFactory httpClientFactory,
        PlantIdApiConfiguration config,
        ProxyConfiguration proxyConfig,
        ILogger<PlantIdService> logger)
    {
        _config = config;
        _logger = logger;
        _identifyEndpoint = config.AnalyzeEndpoint.TrimStart('/');

        if (proxyConfig.Enabled && !string.IsNullOrWhiteSpace(proxyConfig.Host))
        {
            var handler = new SocketsHttpHandler
            {
                Proxy = CreateProxy(proxyConfig)
            };

            _httpClient = new HttpClient(handler);
        }
        else
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        _httpClient.BaseAddress = new Uri(config.BaseUrl.TrimEnd('/'));
        _httpClient.Timeout = TimeSpan.FromSeconds(config.Timeout);
    }

    public async Task<Result<PlantIdAnalyzeResponse>> AnalyzeAsync(
        PlantIdAnalyzeRequest request,
        CancellationToken cancellationToken = default)
    {
        return await CreateIdentificationAsync(request, null, cancellationToken);
    }

    public async Task<Result<PlantIdAnalyzeResponse>> CreateIdentificationAsync(
        PlantIdAnalyzeRequest request,
        PlantIdRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
            return Result.Failure<PlantIdAnalyzeResponse>(
                "Plant.id ApiKey не задан. Укажи PlantId:ApiKey в конфигурации.");

        if (request is null || request.Images.Count == 0)
            return Result.Failure<PlantIdAnalyzeResponse>("Для анализа необходимо передать хотя бы одно изображение.");

        var normalizedRequest = NormalizeRequest(request);
        if (normalizedRequest.Images.Count == 0)
            return Result.Failure<PlantIdAnalyzeResponse>("Для анализа необходимо передать хотя бы одно валидное изображение.");

        return await ExecuteWithRetryAsync(
            async ct =>
            {
                using var httpRequest = CreateJsonRequest(
                    HttpMethod.Post,
                    BuildUri(_identifyEndpoint, options),
                    normalizedRequest);
                using var response = await _httpClient.SendAsync(httpRequest, ct);
                return await ParseResponseAsync<PlantIdAnalyzeResponse>(response, ct);
            },
            cancellationToken);
    }

    public async Task<Result<PlantIdAnalyzeResponse>> GetIdentificationAsync(
        string accessToken,
        PlantIdRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
            return Result.Failure<PlantIdAnalyzeResponse>(
                "Plant.id ApiKey не задан. Укажи PlantId:ApiKey в конфигурации.");

        if (string.IsNullOrWhiteSpace(accessToken))
            return Result.Failure<PlantIdAnalyzeResponse>("access_token обязателен для получения результата.");

        var endpoint = $"{_identifyEndpoint}/{Uri.EscapeDataString(accessToken)}";
        return await ExecuteWithRetryAsync(
            async ct =>
            {
                using var httpRequest = CreateRequest(HttpMethod.Get, BuildUri(endpoint, options));
                using var response = await _httpClient.SendAsync(httpRequest, ct);
                return await ParseResponseAsync<PlantIdAnalyzeResponse>(response, ct);
            },
            cancellationToken);
    }

    public async Task<Result<PlantIdAnalyzeResponse>> CreateHealthAssessmentAsync(
        PlantIdAnalyzeRequest request,
        PlantIdRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var apiKeyError = EnsureApiKeyConfigured<PlantIdAnalyzeResponse>();
        if (apiKeyError is not null)
            return apiKeyError;

        if (request is null || request.Images.Count == 0)
            return Result.Failure<PlantIdAnalyzeResponse>("Для health assessment необходимо передать хотя бы одно изображение.");

        var normalizedRequest = NormalizeRequest(request);
        if (normalizedRequest.Images.Count == 0)
            return Result.Failure<PlantIdAnalyzeResponse>("Для health assessment необходимо передать хотя бы одно валидное изображение.");

        return await ExecuteWithRetryAsync(
            async ct =>
            {
                using var httpRequest = CreateJsonRequest(
                    HttpMethod.Post,
                    BuildUri(HealthAssessmentEndpoint, options),
                    normalizedRequest);
                using var response = await _httpClient.SendAsync(httpRequest, ct);
                return await ParseResponseAsync<PlantIdAnalyzeResponse>(response, ct);
            },
            cancellationToken);
    }

    public async Task<Result<PlantIdEmptyResponse>> DeleteIdentificationAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var apiKeyError = EnsureApiKeyConfigured<PlantIdEmptyResponse>();
        if (apiKeyError is not null)
            return apiKeyError;

        if (string.IsNullOrWhiteSpace(accessToken))
            return Result.Failure<PlantIdEmptyResponse>("access_token обязателен для удаления identification.");

        var endpoint = $"{_identifyEndpoint}/{Uri.EscapeDataString(accessToken)}";
        return await ExecuteWithRetryAsync(
            async ct =>
            {
                using var httpRequest = CreateRequest(HttpMethod.Delete, endpoint);
                using var response = await _httpClient.SendAsync(httpRequest, ct);
                return await ParseResponseAsync<PlantIdEmptyResponse>(response, ct, allowEmptySuccessBody: true);
            },
            cancellationToken);
    }

    public async Task<Result<PlantIdUsageInfoResponse>> GetUsageInfoAsync(
        CancellationToken cancellationToken = default)
    {
        var apiKeyError = EnsureApiKeyConfigured<PlantIdUsageInfoResponse>();
        if (apiKeyError is not null)
            return apiKeyError;

        return await ExecuteWithRetryAsync(
            async ct =>
            {
                using var httpRequest = CreateRequest(HttpMethod.Get, UsageInfoEndpoint);
                using var response = await _httpClient.SendAsync(httpRequest, ct);
                return await ParseResponseAsync<PlantIdUsageInfoResponse>(response, ct);
            },
            cancellationToken);
    }

    public async Task<Result<PlantIdMessageResponse>> SendIdentificationFeedbackAsync(
        string accessToken,
        PlantIdFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKeyError = EnsureApiKeyConfigured<PlantIdMessageResponse>();
        if (apiKeyError is not null)
            return apiKeyError;

        if (string.IsNullOrWhiteSpace(accessToken))
            return Result.Failure<PlantIdMessageResponse>("access_token обязателен для отправки feedback.");
        if (request is null)
            return Result.Failure<PlantIdMessageResponse>("feedback обязателен.");

        var endpoint = $"{_identifyEndpoint}/{Uri.EscapeDataString(accessToken)}/feedback";
        return await ExecuteWithRetryAsync(
            async ct =>
            {
                using var httpRequest = CreateJsonRequest(HttpMethod.Post, endpoint, request);
                using var response = await _httpClient.SendAsync(httpRequest, ct);
                return await ParseResponseAsync<PlantIdMessageResponse>(response, ct, allowEmptySuccessBody: true);
            },
            cancellationToken);
    }

    public async Task<Result<PlantIdPlantSearchResponse>> SearchPlantsAsync(
        PlantIdPlantSearchOptions options,
        CancellationToken cancellationToken = default)
    {
        var apiKeyError = EnsureApiKeyConfigured<PlantIdPlantSearchResponse>();
        if (apiKeyError is not null)
            return apiKeyError;

        if (options is null || string.IsNullOrWhiteSpace(options.Query))
            return Result.Failure<PlantIdPlantSearchResponse>("Для поиска растений необходимо указать query.");

        var endpoint = BuildUri(
            PlantNameSearchEndpoint,
            ("q", options.Query),
            ("limit", options.Limit?.ToString()),
            ("language", options.Language),
            ("thumbnails", options.Thumbnails.HasValue ? (options.Thumbnails.Value ? "true" : "false") : null));

        return await ExecuteWithRetryAsync(
            async ct =>
            {
                using var httpRequest = CreateRequest(HttpMethod.Get, endpoint);
                using var response = await _httpClient.SendAsync(httpRequest, ct);
                return await ParseResponseAsync<PlantIdPlantSearchResponse>(response, ct);
            },
            cancellationToken);
    }

    public async Task<Result<PlantIdKbPlantDetailsResponse>> GetPlantDetailsAsync(
        string accessToken,
        PlantIdRequestOptions options,
        CancellationToken cancellationToken = default)
    {
        var apiKeyError = EnsureApiKeyConfigured<PlantIdKbPlantDetailsResponse>();
        if (apiKeyError is not null)
            return apiKeyError;

        if (string.IsNullOrWhiteSpace(accessToken))
            return Result.Failure<PlantIdKbPlantDetailsResponse>("access_token обязателен для получения plant details.");

        var endpoint = BuildUri(
            $"api/v3/kb/plants/{Uri.EscapeDataString(accessToken)}",
            ("details", options?.Details),
            ("language", options?.Language));

        return await ExecuteWithRetryAsync(
            async ct =>
            {
                using var httpRequest = CreateRequest(HttpMethod.Get, endpoint);
                using var response = await _httpClient.SendAsync(httpRequest, ct);
                return await ParseResponseAsync<PlantIdKbPlantDetailsResponse>(response, ct);
            },
            cancellationToken);
    }

    public async Task<Result<PlantIdConversationResponse>> AskConversationAsync(
        string accessToken,
        PlantIdConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKeyError = EnsureApiKeyConfigured<PlantIdConversationResponse>();
        if (apiKeyError is not null)
            return apiKeyError;

        if (string.IsNullOrWhiteSpace(accessToken))
            return Result.Failure<PlantIdConversationResponse>("access_token обязателен для conversation.");
        if (request is null || string.IsNullOrWhiteSpace(request.Question))
            return Result.Failure<PlantIdConversationResponse>("question обязателен для chatbot conversation.");

        var endpoint = $"{_identifyEndpoint}/{Uri.EscapeDataString(accessToken)}/conversation";
        return await ExecuteWithRetryAsync(
            async ct =>
            {
                using var httpRequest = CreateJsonRequest(HttpMethod.Post, endpoint, request);
                using var response = await _httpClient.SendAsync(httpRequest, ct);
                return await ParseResponseAsync<PlantIdConversationResponse>(response, ct);
            },
            cancellationToken);
    }

    public async Task<Result<PlantIdConversationResponse>> GetConversationAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var apiKeyError = EnsureApiKeyConfigured<PlantIdConversationResponse>();
        if (apiKeyError is not null)
            return apiKeyError;

        if (string.IsNullOrWhiteSpace(accessToken))
            return Result.Failure<PlantIdConversationResponse>("access_token обязателен для conversation.");

        var endpoint = $"{_identifyEndpoint}/{Uri.EscapeDataString(accessToken)}/conversation";
        return await ExecuteWithRetryAsync(
            async ct =>
            {
                using var httpRequest = CreateRequest(HttpMethod.Get, endpoint);
                using var response = await _httpClient.SendAsync(httpRequest, ct);
                return await ParseResponseAsync<PlantIdConversationResponse>(response, ct);
            },
            cancellationToken);
    }

    public async Task<Result<PlantIdConversationResponse>> DeleteConversationAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var apiKeyError = EnsureApiKeyConfigured<PlantIdConversationResponse>();
        if (apiKeyError is not null)
            return apiKeyError;

        if (string.IsNullOrWhiteSpace(accessToken))
            return Result.Failure<PlantIdConversationResponse>("access_token обязателен для conversation.");

        var endpoint = $"{_identifyEndpoint}/{Uri.EscapeDataString(accessToken)}/conversation";
        return await ExecuteWithRetryAsync(
            async ct =>
            {
                using var httpRequest = CreateRequest(HttpMethod.Delete, endpoint);
                using var response = await _httpClient.SendAsync(httpRequest, ct);
                return await ParseResponseAsync<PlantIdConversationResponse>(response, ct);
            },
            cancellationToken);
    }

    public async Task<Result<PlantIdEmptyResponse>> SendConversationFeedbackAsync(
        string accessToken,
        PlantIdConversationFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKeyError = EnsureApiKeyConfigured<PlantIdEmptyResponse>();
        if (apiKeyError is not null)
            return apiKeyError;

        if (string.IsNullOrWhiteSpace(accessToken))
            return Result.Failure<PlantIdEmptyResponse>("access_token обязателен для feedback по conversation.");
        if (request?.Feedback is null)
            return Result.Failure<PlantIdEmptyResponse>("feedback обязателен для отправки.");

        var endpoint = $"{_identifyEndpoint}/{Uri.EscapeDataString(accessToken)}/conversation/feedback";
        return await ExecuteWithRetryAsync(
            async ct =>
            {
                using var httpRequest = CreateJsonRequest(HttpMethod.Post, endpoint, request);
                using var response = await _httpClient.SendAsync(httpRequest, ct);
                return await ParseResponseAsync<PlantIdEmptyResponse>(response, ct, allowEmptySuccessBody: true);
            },
            cancellationToken);
    }

    private async Task<Result<TResponse>> ExecuteWithRetryAsync<TResponse>(
        Func<CancellationToken, Task<PlantIdApiCallResult<TResponse>>> operation,
        CancellationToken cancellationToken)
        where TResponse : class, new()
    {
        var maxAttempts = Math.Max(_config.MaxRetryAttempts, 1);
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var apiResult = await operation(cancellationToken);
                if (apiResult.IsSuccess)
                    return Result.Success(apiResult.Response!);

                if (!apiResult.CanRetry || attempt == maxAttempts)
                    return Result.Failure<TResponse>(apiResult.ErrorMessage!);

                _logger.LogWarning(
                    "Plant.id вернул ошибку (попытка {Attempt}/{MaxAttempts}): {Message}",
                    attempt,
                    maxAttempts,
                    apiResult.ErrorMessage);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                if (attempt == maxAttempts)
                    return Result.Failure<TResponse>(
                        $"Превышен таймаут запроса к Plant.id ({_config.Timeout} сек).");

                _logger.LogWarning(
                    ex,
                    "Таймаут при запросе к Plant.id (попытка {Attempt}/{MaxAttempts})",
                    attempt,
                    maxAttempts);
            }
            catch (HttpRequestException ex)
            {
                if (attempt == maxAttempts)
                    return Result.Failure<TResponse>($"Сетевая ошибка Plant.id: {ex.Message}");

                _logger.LogWarning(
                    ex,
                    "Сетевая ошибка при запросе к Plant.id (попытка {Attempt}/{MaxAttempts})",
                    attempt,
                    maxAttempts);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Не удалось распарсить ответ Plant.id");
                return Result.Failure<TResponse>(
                    "Plant.id вернул ответ в неожиданном формате.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при вызове Plant.id");
                return Result.Failure<TResponse>($"Исключение при вызове Plant.id: {ex.Message}");
            }

            if (attempt < maxAttempts)
                await Task.Delay(_config.RetryDelayMs * attempt, cancellationToken);
        }

        return Result.Failure<TResponse>(
            $"Не удалось выполнить запрос к Plant.id после {maxAttempts} попыток.");
    }

    private HttpRequestMessage CreateJsonRequest<TBody>(HttpMethod method, string uri, TBody request)
        where TBody : class
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var message = CreateRequest(method, uri);
        message.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return message;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string uri)
    {
        var message = new HttpRequestMessage(method, uri);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        message.Headers.TryAddWithoutValidation("Api-Key", _config.ApiKey);
        return message;
    }

    private static PlantIdAnalyzeRequest NormalizeRequest(PlantIdAnalyzeRequest request)
    {
        return new PlantIdAnalyzeRequest
        {
            Images = request.Images
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeImage)
                .ToList(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Health = string.IsNullOrWhiteSpace(request.Health) ? "all" : request.Health,
            SimilarImages = request.SimilarImages ?? true,
            CustomId = request.CustomId,
            Datetime = request.Datetime,
            DiseaseLevel = string.IsNullOrWhiteSpace(request.DiseaseLevel) ? null : request.DiseaseLevel,
            SuggestionFilter = request.SuggestionFilter,
            ClassificationLevel = string.IsNullOrWhiteSpace(request.ClassificationLevel)
                ? null
                : request.ClassificationLevel,
            ClassificationRaw = request.ClassificationRaw,
            Symptoms = request.Symptoms
        };
    }

    private static string NormalizeImage(string image)
    {
        if (image.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            return image;

        return $"data:image/jpeg;base64,{image}";
    }

    private static bool ShouldRetry(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.TooManyRequests ||
               statusCode == HttpStatusCode.RequestTimeout ||
               (int)statusCode >= 500;
    }

    private async Task<PlantIdApiCallResult<TResponse>> ParseResponseAsync<TResponse>(
        HttpResponseMessage response,
        CancellationToken cancellationToken,
        bool allowEmptySuccessBody = false)
        where TResponse : class, new()
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            if (string.IsNullOrWhiteSpace(body))
                return allowEmptySuccessBody
                    ? PlantIdApiCallResult<TResponse>.Success(new TResponse())
                    : PlantIdApiCallResult<TResponse>.Failure("Plant.id вернул пустой ответ.", false);

            var parsed = JsonSerializer.Deserialize<TResponse>(body, JsonOptions);
            if (parsed is null)
                return PlantIdApiCallResult<TResponse>.Failure("Plant.id вернул пустой ответ.", false);

            if (parsed is PlantIdAnalyzeResponse analyzeResponse)
                analyzeResponse.RawJson = body;

            return PlantIdApiCallResult<TResponse>.Success(parsed);
        }

        var errorMessage = TryExtractError(body);
        var message = $"Plant.id вернул HTTP {(int)response.StatusCode}: {errorMessage}";
        return PlantIdApiCallResult<TResponse>.Failure(message, ShouldRetry(response.StatusCode));
    }

    private static string BuildUri(string endpoint, PlantIdRequestOptions? options)
    {
        var normalizedEndpoint = endpoint.TrimStart('/');
        if (options is null)
            return normalizedEndpoint;

        var queryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(options.Details))
            queryParts.Add($"details={Uri.EscapeDataString(options.Details)}");
        if (!string.IsNullOrWhiteSpace(options.Language))
            queryParts.Add($"language={Uri.EscapeDataString(options.Language)}");
        if (options.Async.HasValue)
            queryParts.Add($"async={(options.Async.Value ? "true" : "false")}");

        if (queryParts.Count == 0)
            return normalizedEndpoint;

        var queryString = string.Join("&", queryParts);
        return string.IsNullOrWhiteSpace(queryString)
            ? normalizedEndpoint
            : $"{normalizedEndpoint}?{queryString}";
    }

    private static string BuildUri(string endpoint, params (string Key, string? Value)[] queryParams)
    {
        var normalizedEndpoint = endpoint.TrimStart('/');
        var queryParts = queryParams
            .Where(static x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value!)}")
            .ToList();

        if (queryParts.Count == 0)
            return normalizedEndpoint;

        return $"{normalizedEndpoint}?{string.Join("&", queryParts)}";
    }

    private static IWebProxy? CreateProxy(ProxyConfiguration proxyConfig)
    {
        if (!proxyConfig.Enabled || string.IsNullOrWhiteSpace(proxyConfig.Host))
            return null;

        var proxyUri = $"{proxyConfig.Protocol.ToLowerInvariant()}://{proxyConfig.Host}:{proxyConfig.Port}";
        var webProxy = new WebProxy(proxyUri);

        if (!string.IsNullOrWhiteSpace(proxyConfig.Username) &&
            !string.IsNullOrWhiteSpace(proxyConfig.Password))
        {
            webProxy.Credentials = new NetworkCredential(proxyConfig.Username, proxyConfig.Password);
        }

        return webProxy;
    }

    private static string TryExtractError(string body)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<PlantIdErrorResponse>(body, JsonOptions);
            return parsed?.Message ?? parsed?.Error ?? parsed?.Detail ?? body;
        }
        catch
        {
            return body;
        }
    }

    private Result<TResponse>? EnsureApiKeyConfigured<TResponse>() where TResponse : class, new()
    {
        return string.IsNullOrWhiteSpace(_config.ApiKey)
            ? Result.Failure<TResponse>("Plant.id ApiKey не задан. Укажи PlantId:ApiKey в конфигурации.")
            : null;
    }

    private sealed class PlantIdApiCallResult<TResponse> where TResponse : class
    {
        public bool IsSuccess { get; private init; }
        public bool CanRetry { get; private init; }
        public string? ErrorMessage { get; private init; }
        public TResponse? Response { get; private init; }

        public static PlantIdApiCallResult<TResponse> Success(TResponse response) =>
            new()
            {
                IsSuccess = true,
                Response = response
            };

        public static PlantIdApiCallResult<TResponse> Failure(string message, bool canRetry) =>
            new()
            {
                IsSuccess = false,
                ErrorMessage = message,
                CanRetry = canRetry
            };
    }
}
