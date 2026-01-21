using System.Security.Claims;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using GROUPFLOW.Features.Chat.Entities;

public class EntrySubscription
{
    [Authorize]
    [Subscribe]
    public Entry OnEntryAdded(
        int chatId,
        ClaimsPrincipal user,
        [EventMessage] Entry entry)
    {
        // 🔐 dodatkowa walidacja (opcjonalna)
        // sprawdź czy user należy do chatId

        return entry;
    }

    [Authorize]
    [Subscribe]
    [Topic("CHAT_{chatId}")]
    public Entry OnPublicEntry(
        int chatId,
        [EventMessage] Entry entry)
        => entry;

    [Authorize]
    [Subscribe]
    [Topic("USERCHAT_{userChatId}")]
    public Entry OnPrivateEntry(
        int userChatId,
        [EventMessage] Entry entry)
        => entry;
}
