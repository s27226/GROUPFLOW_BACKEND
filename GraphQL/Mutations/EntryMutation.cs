using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class EntryMutation
{
    public async Task<Entry> CreateEntry(
        AppDbContext context,
        EntryInput input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.Message))
        {
            throw new GraphQLException("Message cannot be empty");
        }

        var entry = new Entry
        {
            UserChatId = input.UserChatId,
            Message = input.Message,
            Sent = DateTime.UtcNow,
            Public = input.Public
        };

        context.Entries.Add(entry);
        await context.SaveChangesAsync(cancellationToken);

        var result = await context.Entries
            .Include(e => e.UserChat)
            .ThenInclude(uc => uc.User)
            .FirstOrDefaultAsync(e => e.Id == entry.Id, cancellationToken);

        if (result == null)
        {
            throw new GraphQLException("Failed to load created entry");
        }

        return result;
    }
}
