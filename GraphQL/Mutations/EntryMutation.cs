using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class EntryMutation
{
    public Entry CreateEntry(AppDbContext context, EntryInput input)
    {
        var entry = new Entry
        {
            UserChatId = input.UserChatId,
            Message = input.Message,
            Sent = DateTime.UtcNow,
            Public = input.Public
        };
        context.Entries.Add(entry);
        context.SaveChanges();
        
        // Reload entry with navigation properties
        return context.Entries
            .Include(e => e.UserChat)
                .ThenInclude(uc => uc.User)
            .First(e => e.Id == entry.Id);
    }

    public Entry? UpdateEntry(AppDbContext context, UpdateEntryInput input)
    {
        var entry = context.Entries.Find(input.Id);
        if (entry == null) return null;
        if (!string.IsNullOrEmpty(input.Message)) entry.Message = input.Message;
        if (input.Public.HasValue) entry.Public = input.Public.Value;
        context.SaveChanges();
        return entry;
    }

    public bool DeleteEntry(AppDbContext context, int id)
    {
        var entry = context.Entries.Find(id);
        if (entry == null) return false;
        context.Entries.Remove(entry);
        context.SaveChanges();
        return true;
    }
}