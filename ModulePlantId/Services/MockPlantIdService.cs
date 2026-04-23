using ModulePlantId.Models;
using Shared.Contracts;

namespace ModulePlantId.Services;

public class MockPlantIdService : IPlantIdService
{
    public Task<Result<PlantIdAnalyzeResponse>> AnalyzeAsync(
        PlantIdAnalyzeRequest request,
        CancellationToken cancellationToken = default)
    {
        return CreateIdentificationAsync(request, null, cancellationToken);
    }

    public Task<Result<PlantIdAnalyzeResponse>> CreateIdentificationAsync(
        PlantIdAnalyzeRequest request,
        PlantIdRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = new PlantIdAnalyzeResponse
        {
            AccessToken = "XFMRsKSGe62ODXd",
            ModelVersion = "plant_id:5.1.1",
            CustomId = null,
            Input = new PlantIdInputData
            {
                Latitude = 49.207,
                Longitude = 16.608,
                Health = "all",
                SimilarImages = true,
                Images =
                [
                    "https://plant.id/media/imgs/bac766beb9f5418aac3d919e0ef54d89.jpg"
                ],
                Datetime = DateTimeOffset.Parse("2026-04-23T13:48:32.195343+00:00")
            },
            Result = new PlantIdResultData
            {
                Disease = new PlantIdDiseaseData
                {
                    Suggestions =
                    [
                        new PlantIdSuggestion
                        {
                            Id = "5ce70d29fa1d9561",
                            Name = "Pucciniales",
                            Probability = 0.0227,
                            SimilarImages =
                            [
                                new PlantIdSimilarImage
                                {
                                    Id = "948366579a10097851ef86e6743e279f7f110182",
                                    Url = "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/948/366579a10097851ef86e6743e279f7f110182.jpeg",
                                    LicenseName = "CC BY 3.0",
                                    LicenseUrl = "https://creativecommons.org/licenses/by/3.0/",
                                    Citation = "Johan Adler",
                                    Similarity = 0.38,
                                    UrlSmall = "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/948/366579a10097851ef86e6743e279f7f110182.small.jpeg"
                                },
                                new PlantIdSimilarImage
                                {
                                    Id = "2a95e94819e4ea61016afbdc6d956946c994d266",
                                    Url = "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/2a9/5e94819e4ea61016afbdc6d956946c994d266.jpg",
                                    LicenseName = "CC BY 3.0",
                                    LicenseUrl = "https://creativecommons.org/licenses/by/3.0/",
                                    Citation = "Jared Lincenberg",
                                    Similarity = 0.374,
                                    UrlSmall = "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/2a9/5e94819e4ea61016afbdc6d956946c994d266.small.jpg"
                                }
                            ],
                            Details = new PlantIdDetails
                            {
                                Language = "en",
                                EntityId = "5ce70d29fa1d9561"
                            }
                        }
                    ],
                    Question = null
                },
                Classification = new PlantIdClassificationData
                {
                    Suggestions =
                    [
                        new PlantIdSuggestion
                        {
                            Id = "ae8faed4a61d9de2",
                            Name = "Leucojum vernum",
                            Probability = 0.99,
                            SimilarImages =
                            [
                                new PlantIdSimilarImage
                                {
                                    Id = "5c56885e75c845d7b1ca2034e8f7cdcb2715e8fa",
                                    Url = "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/5c5/6885e75c845d7b1ca2034e8f7cdcb2715e8fa.jpg",
                                    Similarity = 0.775,
                                    UrlSmall = "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/5c5/6885e75c845d7b1ca2034e8f7cdcb2715e8fa.small.jpg"
                                },
                                new PlantIdSimilarImage
                                {
                                    Id = "0d3a111fcec1035892224e7b3045db73ecfbfd9e",
                                    Url = "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/0d3/a111fcec1035892224e7b3045db73ecfbfd9e.jpg",
                                    LicenseName = "CC BY 4.0",
                                    LicenseUrl = "https://creativecommons.org/licenses/by/4.0/",
                                    Citation = "Dao Nguyen and James Hardcastle",
                                    Similarity = 0.771,
                                    UrlSmall = "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/0d3/a111fcec1035892224e7b3045db73ecfbfd9e.small.jpg"
                                }
                            ],
                            Details = new PlantIdDetails
                            {
                                Language = "en",
                                EntityId = "ae8faed4a61d9de2"
                            }
                        }
                    ]
                },
                IsPlant = new PlantIdBinaryResult
                {
                    Probability = 0.9580986,
                    Threshold = 0.5,
                    Binary = true
                },
                IsHealthy = new PlantIdBinaryResult
                {
                    Binary = true,
                    Probability = 0.9702000286051771,
                    Threshold = 0.5
                }
            },
            Status = "COMPLETED",
            SlaCompliantClient = true,
            SlaCompliantSystem = true,
            Created = 1776952112.195343,
            Completed = 1776952112.807876
        };

        return Task.FromResult(Result.Success(response));
    }

    public Task<Result<PlantIdAnalyzeResponse>> GetIdentificationAsync(
        string accessToken,
        PlantIdRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return Task.FromResult(Result.Failure<PlantIdAnalyzeResponse>("access_token обязателен для получения результата."));

        return CreateIdentificationAsync(
            new PlantIdAnalyzeRequest { Images = ["data:image/jpeg;base64,mock"] },
            options,
            cancellationToken);
    }

    public Task<Result<PlantIdAnalyzeResponse>> CreateHealthAssessmentAsync(
        PlantIdAnalyzeRequest request,
        PlantIdRequestOptions? options = null,
        CancellationToken cancellationToken = default) =>
        CreateIdentificationAsync(request, options, cancellationToken);

    public Task<Result<PlantIdEmptyResponse>> DeleteIdentificationAsync(
        string accessToken,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new PlantIdEmptyResponse()));

    public Task<Result<PlantIdUsageInfoResponse>> GetUsageInfoAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new PlantIdUsageInfoResponse
        {
            Active = true,
            CreditLimits = new PlantIdCreditCounter { Total = 1000 },
            Used = new PlantIdCreditCounter { Day = 1, Week = 3, Month = 10, Total = 42 },
            Remaining = new PlantIdCreditCounter { Total = 958 },
            CanUseCredits = new PlantIdCanUseCredits { Value = true }
        }));

    public Task<Result<PlantIdMessageResponse>> SendIdentificationFeedbackAsync(
        string accessToken,
        PlantIdFeedbackRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new PlantIdMessageResponse { Message = "Feedback submitted successfully." }));

    public Task<Result<PlantIdPlantSearchResponse>> SearchPlantsAsync(
        PlantIdPlantSearchOptions options,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new PlantIdPlantSearchResponse
        {
            Limit = options.Limit ?? 10,
            EntitiesTrimmed = false,
            Entities =
            [
                new PlantIdPlantEntitySearchResult
                {
                    EntityName = "Tulipa gesneriana",
                    MatchedIn = "Tulipa gesneriana",
                    MatchedInType = "entity_name",
                    AccessToken = "mock-token"
                }
            ]
        }));

    public Task<Result<PlantIdKbPlantDetailsResponse>> GetPlantDetailsAsync(
        string accessToken,
        PlantIdRequestOptions options,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new PlantIdKbPlantDetailsResponse
        {
            EntityId = accessToken,
            Language = options.Language ?? "en"
        }));

    public Task<Result<PlantIdConversationResponse>> AskConversationAsync(
        string accessToken,
        PlantIdConversationRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(CreateMockConversation(accessToken, request.Question)));

    public Task<Result<PlantIdConversationResponse>> GetConversationAsync(
        string accessToken,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(CreateMockConversation(accessToken, "What is this plant?")));

    public Task<Result<PlantIdConversationResponse>> DeleteConversationAsync(
        string accessToken,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(CreateMockConversation(accessToken, "Conversation removed.")));

    public Task<Result<PlantIdEmptyResponse>> SendConversationFeedbackAsync(
        string accessToken,
        PlantIdConversationFeedbackRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new PlantIdEmptyResponse()));

    private static PlantIdConversationResponse CreateMockConversation(string accessToken, string question)
    {
        return new PlantIdConversationResponse
        {
            Identification = accessToken,
            RemainingCalls = 19,
            ModelParameters = new PlantIdConversationModelParameters
            {
                Model = "gpt-4o-mini",
                Temperature = 0.5
            },
            Messages =
            [
                new PlantIdConversationMessage
                {
                    Type = "question",
                    Content = question,
                    Created = DateTimeOffset.UtcNow
                },
                new PlantIdConversationMessage
                {
                    Type = "answer",
                    Content = "Это mock-ответ чат-бота Plant.id.",
                    Created = DateTimeOffset.UtcNow
                }
            ]
        };
    }
}
