using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Chat.Entities;

namespace GROUPFLOW.Features.Chat.GraphQL.Queries;

public class EntryQuery
{
    [GraphQLName("allentries")]
    public async Task<List<Entry>> GetEntries(AppDbContext context) 
    {
        return await context.Entries
            .Include(e => e.UserChat)
                .ThenInclude(uc => uc.User)
                    .ThenInclude(u => u.ProfilePicBlob)
            .ToListAsync();
    }
    
    // [GraphQLName("entrybyid")]
    // public Entry? GetEntryById(AppDbContext context, int id) => context.Entries.FirstOrDefault(g => g.Id == id);
    
    /// <summary>
    /// Get all messages for a specific chat, ordered by sent time.
    /// Works for both project chats and direct messages.
    /// </summary>
    [GraphQLName("chatmessages")]
    public async Task<List<Entry>> GetChatMessages(AppDbContext context, int chatId)
    {
        return await context.Entries
            .Include(e => e.UserChat)
                .ThenInclude(uc => uc.User)
                    .ThenInclude(u => u.ProfilePicBlob)
            .Where(e => e.UserChat.ChatId == chatId)
            .OrderBy(e => e.Sent)
            .ToListAsync();
    }
}
