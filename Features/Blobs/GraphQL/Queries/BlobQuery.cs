using HotChocolate;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.GraphQL;
using GROUPFLOW.Features.Blobs.Entities;

namespace GROUPFLOW.Features.Blobs.GraphQL.Queries;

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
