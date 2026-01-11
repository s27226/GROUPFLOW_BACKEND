using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.GraphQL;
using GROUPFLOW.Features.Chat.Entities;
using GROUPFLOW.Features.Chat.Inputs;

namespace GROUPFLOW.Features.Chat.GraphQL;

/// <summary>
/// GraphQL mutations for chat entry operations.
/// </summary>
public class EntryMutation
{
    public async Task<Entry> CreateEntry(
        [Service] AppDbContext context,
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

        context.Entries.Add(entry);
        await context.SaveChangesAsync(ct);

        // Reload entry with navigation properties
        return await context.Entries
            .Include(e => e.UserChat)
                .ThenInclude(uc => uc.User)
            .FirstAsync(e => e.Id == entry.Id, ct);
    }
}
