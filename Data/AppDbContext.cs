using System.Reflection;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.Data;

/// <summary>
/// Application database context.
/// Entity configurations are defined in separate files under Data/Configurations/.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // User-related entities
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<UserInterest> UserInterests => Set<UserInterest>();
    
    // Friendship-related entities
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<FriendRecommendation> FriendRecommendations => Set<FriendRecommendation>();
    public DbSet<BlockedUser> BlockedUsers => Set<BlockedUser>();
    
    // Project-related entities
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<UserProject> UserProjects => Set<UserProject>();
    public DbSet<ProjectInvitation> ProjectInvitations => Set<ProjectInvitation>();
    public DbSet<ProjectRecommendation> ProjectRecommendations => Set<ProjectRecommendation>();
    public DbSet<ProjectEvent> ProjectEvents => Set<ProjectEvent>();
    public DbSet<ProjectLike> ProjectLikes => Set<ProjectLike>();
    public DbSet<ProjectView> ProjectViews => Set<ProjectView>();
    public DbSet<ProjectSkill> ProjectSkills => Set<ProjectSkill>();
    public DbSet<ProjectInterest> ProjectInterests => Set<ProjectInterest>();
    
    // Post-related entities
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<PostComment> PostComments => Set<PostComment>();
    public DbSet<PostCommentLike> PostCommentLikes => Set<PostCommentLike>();
    public DbSet<PostReport> PostReports => Set<PostReport>();
    public DbSet<SavedPost> SavedPosts => Set<SavedPost>();
    
    // Chat-related entities
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<UserChat> UserChats => Set<UserChat>();
    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<EntryReaction> EntryReactions => Set<EntryReaction>();
    public DbSet<ReadBy> ReadBys => Set<ReadBy>();
    public DbSet<SharedFile> SharedFiles => Set<SharedFile>();
    public DbSet<Emote> Emotes => Set<Emote>();
    
    // Other entities
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<BlobFile> BlobFiles => Set<BlobFile>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // Only configure if not already configured (e.g., in tests)
        if (!options.IsConfigured)
        {
            options.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONN_STRING")
                ?? throw new InvalidOperationException("POSTGRES_CONN_STRING not found in environment variables."));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all IEntityTypeConfiguration<T> classes from this assembly
        // Configurations are located in Data/Configurations/ folder
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
