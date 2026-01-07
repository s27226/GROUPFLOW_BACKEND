using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class EntryMutation
{
    private readonly AppDbContext _context;

    public EntryMutation(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Entry> CreateEntry(
        EntryInput input,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
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

            _context.Entries.Add(entry);
            await _context.SaveChangesAsync(cancellationToken);

            var result = await _context.Entries
                .Include(e => e.UserChat)
                .ThenInclude(uc => uc.User)
                .FirstOrDefaultAsync(e => e.Id == entry.Id, cancellationToken);

            if (result == null)
            {
                throw new GraphQLException("Failed to load created entry");
            }

            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
