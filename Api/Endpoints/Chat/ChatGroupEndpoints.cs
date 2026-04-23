using FastEndpoints;

namespace Api.Endpoints.Chat;

public sealed class ChatGroupEndpoints : Group
{
    public ChatGroupEndpoints()
    {
        Configure("chats", _ => { });
    }
}
