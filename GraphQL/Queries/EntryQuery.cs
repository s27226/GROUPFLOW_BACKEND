using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class EntryQuery
{
    private readonly AppDbContext _context;

    public EntryQuery(AppDbContext context)
    {
        _context = context;
    }

    [GraphQLName("allentries")]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Entry> GetEntries() => _context.Entries;
    
    // [GraphQLName("entrybyid")]
    // [UseProjection]
    // public Entry? GetEntryById(AppDbContext context, int id) => context.Entries.FirstOrDefault(g => g.Id == id);
    
    /// <summary>
    /// Get all messages for a specific chat, ordered by sent time.
    /// Works for both project chats and direct messages.
    /// </summary>
    [GraphQLName("chatmessages")]
    [UseProjection]
    public IQueryable<Entry> GetChatMessages(int chatId)
    {
        return _context.Entries
            .Where(e => e.UserChat.ChatId == chatId)
            .OrderBy(e => e.Sent);
    }
}