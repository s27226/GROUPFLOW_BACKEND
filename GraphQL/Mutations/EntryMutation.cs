using GroupFlow_BACKEND.Data;
using GroupFlow_BACKEND.GraphQL.Inputs;
using GroupFlow_BACKEND.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.GraphQL.Mutations;

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