using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.GraphQL;
using GROUPFLOW.Features.Chat.Entities;
using GROUPFLOW.Features.Chat.GraphQL.Inputs;
using HotChocolate.Subscriptions;

namespace GROUPFLOW.Features.Chat.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for chat entry operations.
/// </summary>
public class EntryMutation
{
    public async Task<Entry> CreateEntry(
        [Service] AppDbContext context,
        [Service] ITopicEventSender eventSender,
        EntryInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();

        var entry = new Entry
        {
            UserChatId = input.UserChatId,
            Message = input.Message,
            Sent = DateTime.UtcNow,
            Public = input.Public
        };
        
        var fullEntry= await context.Entries
                                   .Include(e => e.UserChat)
                                       .ThenInclude(uc => uc.User)
                                   .FirstAsync(e => e.Id == entry.Id, ct);

        await eventSender.SendAsync(
            GetTopic(fullEntry),
            fullEntry,
            ct);

        context.Entries.Add(entry);
        await context.SaveChangesAsync(ct);

        // Reload entry with navigation properties
        return fullEntry;
    }
    
    private static string GetTopic(Entry entry)
    {
        // Publiczny czat → cały Chat
        if (entry.Public)
            return $"CHAT_{entry.UserChat.ChatId}";

        // Prywatny → tylko konkretna relacja UserChat
        return $"USERCHAT_{entry.UserChatId}";
    }
}
