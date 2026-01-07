using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Queries;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND;

public class Query
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Query(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public ChatQuery Chat => new(_context, _httpContextAccessor);
    
    public EntryQuery Entry => new(_context);
    
    
    public FriendRequestQuery FriendRequest => new(_context, _httpContextAccessor);
    public ProjectInvitationQuery ProjectInvitation => new(_context, _httpContextAccessor);
    public ProjectRecommendationQuery ProjectRecommendation => new(_context);
    
    public UserChatQuery UserChat => new(_context, _httpContextAccessor);
    
    public UsersQuery Users => new(_context);
    public PostQuery Post => new(_context, _httpContextAccessor);
    public ProjectQuery Project => new(_context, _httpContextAccessor);
    public FriendshipQuery Friendship => new(_context, _httpContextAccessor);
    public ProjectEventQuery ProjectEvent => new(_context);
    public SavedPostQuery SavedPost => new(_context, _httpContextAccessor);
    public UserTagQuery UserTag => new(_context, _httpContextAccessor);
    public NotificationQuery Notification => new(_context, _httpContextAccessor);
    public BlockedUserQuery BlockedUser => new(_context, _httpContextAccessor);
    public AdminQuery Admin => new(_context, _httpContextAccessor);
    public ModerationQuery Moderation => new(_context, _httpContextAccessor);
}