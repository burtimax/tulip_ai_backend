using ModulePlantId.Models;
using Shared.Contracts;

namespace ModulePlantId.Services;

public interface IPlantIdService
{
    Task<Result<PlantIdAnalyzeResponse>> AnalyzeAsync(
        PlantIdAnalyzeRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PlantIdAnalyzeResponse>> CreateIdentificationAsync(
        PlantIdAnalyzeRequest request,
        PlantIdRequestOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<Result<PlantIdAnalyzeResponse>> GetIdentificationAsync(
        string accessToken,
        PlantIdRequestOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<Result<PlantIdAnalyzeResponse>> CreateHealthAssessmentAsync(
        PlantIdAnalyzeRequest request,
        PlantIdRequestOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<Result<PlantIdEmptyResponse>> DeleteIdentificationAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<Result<PlantIdUsageInfoResponse>> GetUsageInfoAsync(
        CancellationToken cancellationToken = default);

    Task<Result<PlantIdMessageResponse>> SendIdentificationFeedbackAsync(
        string accessToken,
        PlantIdFeedbackRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PlantIdPlantSearchResponse>> SearchPlantsAsync(
        PlantIdPlantSearchOptions options,
        CancellationToken cancellationToken = default);

    Task<Result<PlantIdKbPlantDetailsResponse>> GetPlantDetailsAsync(
        string accessToken,
        PlantIdRequestOptions options,
        CancellationToken cancellationToken = default);

    Task<Result<PlantIdConversationResponse>> AskConversationAsync(
        string accessToken,
        PlantIdConversationRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PlantIdConversationResponse>> GetConversationAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<Result<PlantIdConversationResponse>> DeleteConversationAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<Result<PlantIdEmptyResponse>> SendConversationFeedbackAsync(
        string accessToken,
        PlantIdConversationFeedbackRequest request,
        CancellationToken cancellationToken = default);
}
