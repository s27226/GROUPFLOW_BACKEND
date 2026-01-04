using GroupFlow_BACKEND.Data;
using GroupFlow_BACKEND.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.GraphQL.Queries;

public class EntryQuery
{
    [GraphQLName("allentries")]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Entry> GetEntries(AppDbContext context) => context.Entries;
    
    // [GraphQLName("entrybyid")]
    // [UseProjection]
    // public Entry? GetEntryById(AppDbContext context, int id) => context.Entries.FirstOrDefault(g => g.Id == id);
    
    /// <summary>
    /// Get all messages for a specific chat, ordered by sent time.
    /// Works for both project chats and direct messages.
    /// </summary>
    [GraphQLName("chatmessages")]
    [UseProjection]
    public IQueryable<Entry> GetChatMessages(AppDbContext context, int chatId)
    {
        return context.Entries
            .Where(e => e.UserChat.ChatId == chatId)
            .OrderBy(e => e.Sent);
    }
}