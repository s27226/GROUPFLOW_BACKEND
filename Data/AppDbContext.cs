using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
{
}

    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<FriendRecommendation> FriendRecommendations => Set<FriendRecommendation>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupInvitation> GroupInvitations => Set<GroupInvitation>();
    public DbSet<GroupRecommendation> GroupRecommendations => Set<GroupRecommendation>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<UserChat> UserChats => Set<UserChat>();
    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<EntryReaction> EntryReactions => Set<EntryReaction>();
    public DbSet<ReadBy> ReadBys => Set<ReadBy>();
    public DbSet<SharedFile> SharedFiles => Set<SharedFile>();
    public DbSet<Emote> Emotes => Set<Emote>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<UserProject> UserProjects => Set<UserProject>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    
    

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

        // Configure Friendship relationships
        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.User)
            .WithMany(u => u.InitiatedFriendships)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Friend)
            .WithMany(u => u.ReceivedFriendships)
            .HasForeignKey(f => f.FriendId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure UserProject relationships
        modelBuilder.Entity<UserProject>()
            .HasKey(up => new { up.UserId, up.ProjectId });

        modelBuilder.Entity<UserProject>()
            .HasOne(up => up.User)
            .WithMany(u => u.ProjectCollaborations)
            .HasForeignKey(up => up.UserId);

        modelBuilder.Entity<UserProject>()
            .HasOne(up => up.Project)
            .WithMany(p => p.Collaborators)
            .HasForeignKey(up => up.ProjectId);

        // Configure Project relationships
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Post relationships - fix potential issues
        modelBuilder.Entity<Post>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Post>()
            .HasOne(p => p.Group)
            .WithMany(g => g.Posts)
            .HasForeignKey(p => p.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure FriendRequest relationships to avoid conflicts
        modelBuilder.Entity<FriendRequest>()
            .HasOne(fr => fr.Requester)
            .WithMany()
            .HasForeignKey(fr => fr.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FriendRequest>()
            .HasOne(fr => fr.Requestee)
            .WithMany()
            .HasForeignKey(fr => fr.RequesteeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
