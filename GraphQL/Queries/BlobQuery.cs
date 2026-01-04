using GroupFlow_BACKEND.Data;
using GroupFlow_BACKEND.Models;
using HotChocolate;
using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.GraphQL.Queries
{
    [ExtendObjectType(typeof(Query))]
    public class BlobQuery
    {
        /// <summary>
        /// Get all files for a specific project
        /// </summary>
        public async Task<List<BlobFile>> GetProjectFiles(
            int projectId,
            [Service] AppDbContext context)
        {
            return await context.BlobFiles
                .Where(b => b.ProjectId == projectId && b.Type == BlobType.ProjectFile && !b.IsDeleted)
                .Include(b => b.UploadedBy)
                .OrderByDescending(b => b.UploadedAt)
                .ToListAsync();
        }
    }
}
