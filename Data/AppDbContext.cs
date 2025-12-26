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
    public DbSet<ProjectInvitation> ProjectInvitations => Set<ProjectInvitation>();
    public DbSet<ProjectRecommendation> ProjectRecommendations => Set<ProjectRecommendation>();
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
    public DbSet<ProjectEvent> ProjectEvents => Set<ProjectEvent>();
    public DbSet<SavedPost> SavedPosts => Set<SavedPost>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<UserInterest> UserInterests => Set<UserInterest>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<PostComment> PostComments => Set<PostComment>();
    public DbSet<PostCommentLike> PostCommentLikes => Set<PostCommentLike>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ProjectLike> ProjectLikes => Set<ProjectLike>();
    public DbSet<ProjectView> ProjectViews => Set<ProjectView>();
    public DbSet<ProjectSkill> ProjectSkills => Set<ProjectSkill>();
    public DbSet<ProjectInterest> ProjectInterests => Set<ProjectInterest>();
    public DbSet<BlockedUser> BlockedUsers => Set<BlockedUser>();
    public DbSet<PostReport> PostReports => Set<PostReport>();
    
    

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

        // Configure User BannedBy relationship
        modelBuilder.Entity<User>()
            .HasOne(u => u.BannedBy)
            .WithMany()
            .HasForeignKey(u => u.BannedByUserId)
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
            .HasOne(p => p.Project)
            .WithMany(proj => proj.Posts)
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure self-referencing relationship for shared posts
        modelBuilder.Entity<Post>()
            .HasOne(p => p.SharedPost)
            .WithMany()
            .HasForeignKey(p => p.SharedPostId)
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

        // Configure SavedPost relationships
        modelBuilder.Entity<SavedPost>()
            .HasKey(sp => new { sp.UserId, sp.PostId });

        modelBuilder.Entity<SavedPost>()
            .HasOne(sp => sp.User)
            .WithMany(u => u.SavedPosts)
            .HasForeignKey(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SavedPost>()
            .HasOne(sp => sp.Post)
            .WithMany(p => p.SavedBy)
            .HasForeignKey(sp => sp.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure UserSkill relationships
        modelBuilder.Entity<UserSkill>()
            .HasOne(us => us.User)
            .WithMany(u => u.Skills)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure UserInterest relationships
        modelBuilder.Entity<UserInterest>()
            .HasOne(ui => ui.User)
            .WithMany(u => u.Interests)
            .HasForeignKey(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure PostComment self-referencing relationship for replies
        modelBuilder.Entity<PostComment>()
            .HasOne(pc => pc.ParentComment)
            .WithMany(pc => pc.Replies)
            .HasForeignKey(pc => pc.ParentCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure PostComment relationships
        modelBuilder.Entity<PostComment>()
            .HasOne(pc => pc.User)
            .WithMany()
            .HasForeignKey(pc => pc.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PostComment>()
            .HasOne(pc => pc.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(pc => pc.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure PostLike relationships
        modelBuilder.Entity<PostLike>()
            .HasOne(pl => pl.User)
            .WithMany()
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PostLike>()
            .HasOne(pl => pl.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure PostCommentLike relationships
        modelBuilder.Entity<PostCommentLike>()
            .HasOne(pcl => pcl.User)
            .WithMany()
            .HasForeignKey(pcl => pcl.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PostCommentLike>()
            .HasOne(pcl => pcl.PostComment)
            .WithMany(pc => pc.Likes)
            .HasForeignKey(pcl => pcl.PostCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Notification relationships
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.ActorUser)
            .WithMany()
            .HasForeignKey(n => n.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Post)
            .WithMany()
            .HasForeignKey(n => n.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure ProjectLike relationships
        modelBuilder.Entity<ProjectLike>()
            .HasOne(pl => pl.User)
            .WithMany()
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectLike>()
            .HasOne(pl => pl.Project)
            .WithMany(p => p.Likes)
            .HasForeignKey(pl => pl.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure unique constraint: one like per user per project
        modelBuilder.Entity<ProjectLike>()
            .HasIndex(pl => new { pl.UserId, pl.ProjectId })
            .IsUnique();

        // Configure ProjectView relationships
        modelBuilder.Entity<ProjectView>()
            .HasOne(pv => pv.User)
            .WithMany()
            .HasForeignKey(pv => pv.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectView>()
            .HasOne(pv => pv.Project)
            .WithMany(p => p.Views)
            .HasForeignKey(pv => pv.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure unique constraint: one view per user per project per day
        modelBuilder.Entity<ProjectView>()
            .HasIndex(pv => new { pv.UserId, pv.ProjectId, pv.ViewDate })
            .IsUnique();

        // Configure BlockedUser relationships
        modelBuilder.Entity<BlockedUser>()
            .HasOne(bu => bu.User)
            .WithMany()
            .HasForeignKey(bu => bu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BlockedUser>()
            .HasOne(bu => bu.Blocked)
            .WithMany()
            .HasForeignKey(bu => bu.BlockedUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure unique constraint: one block per user pair
        modelBuilder.Entity<BlockedUser>()
            .HasIndex(bu => new { bu.UserId, bu.BlockedUserId })
            .IsUnique();

        // Configure PostReport relationships
        modelBuilder.Entity<PostReport>()
            .HasOne(pr => pr.Post)
            .WithMany()
            .HasForeignKey(pr => pr.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostReport>()
            .HasOne(pr => pr.ReportedByUser)
            .WithMany()
            .HasForeignKey(pr => pr.ReportedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
