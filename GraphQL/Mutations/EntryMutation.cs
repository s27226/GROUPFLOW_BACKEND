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

    
}