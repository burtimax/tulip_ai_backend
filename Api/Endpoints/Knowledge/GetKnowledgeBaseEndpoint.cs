using Api.Endpoints.App;
using Application.Services.Knowledge;
using FastEndpoints;
using Shared.Contracts;

namespace Api.Endpoints.Knowledge;

public sealed class GetKnowledgeBaseEndpoint : EndpointWithoutRequest<Result<IReadOnlyList<KnowledgeCategoryDto>>>
{
    private readonly IKnowledgeService _knowledgeService;

    public GetKnowledgeBaseEndpoint(IKnowledgeService knowledgeService)
    {
        _knowledgeService = knowledgeService;
    }

    public override void Configure()
    {
        Get("get");
        Group<KnowledgeGroupEndpoint>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Получение базы знаний";
            s.Description = "Возвращает все категории базы знаний вместе со статьями.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var categories = await _knowledgeService.GetKnowledgeBaseAsync(ct);
        var response = categories
            .Select(category => new KnowledgeCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Order = category.Order,
                Posts = category.Titles
                    .Select(title => new KnowledgePostDto
                    {
                        Id = title.Id,
                        Title = title.Title,
                        Order = title.Order,
                        Excerpt = title.Excerpt,
                        Images = title.Images,
                        Content = title.Content
                    })
                    .ToList()
            })
            .ToList();

        await SendAsync(Result.Success<IReadOnlyList<KnowledgeCategoryDto>>(response), cancellation: ct);
    }
}

public sealed class KnowledgeCategoryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required int Order { get; init; }
    public required IReadOnlyList<KnowledgePostDto> Posts { get; init; }
}

public sealed class KnowledgePostDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required int Order { get; init; }
    public string? Excerpt { get; init; }
    public required IReadOnlyList<string> Images { get; init; }
    public required string Content { get; init; }
}
