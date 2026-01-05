using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Services;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class BlockedUserMutation
{
    private readonly BlockedUserService _service;

    public BlockedUserMutation(BlockedUserService service)
    {
        _service = service;
    }

    
    public Task<BlockedUser> BlockUser(ClaimsPrincipal claimsPrincipal, int userIdToBlock)
    {
        int userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return _service.BlockUser(userId, userIdToBlock);
    }

    
    public Task<bool> UnblockUser(ClaimsPrincipal claimsPrincipal, int userIdToUnblock)
    {
        int userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return _service.UnblockUser(userId, userIdToUnblock);
    }
}
