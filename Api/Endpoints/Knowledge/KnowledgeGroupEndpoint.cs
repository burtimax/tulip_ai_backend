using FastEndpoints;

namespace Api.Endpoints.App;

public sealed class KnowledgeGroupEndpoint : Group
{
    public KnowledgeGroupEndpoint()
    {
        Configure("knowledge", c =>
        {
            //c.Description(d => d.WithTags("application"));
        });
    }
}
