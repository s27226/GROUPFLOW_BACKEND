using GROUPFLOW.Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GROUPFLOW.Features.Users.Entities;
using GROUPFLOW.Features.Posts.Entities;
using GROUPFLOW.Features.Projects.Entities;
using GROUPFLOW.Features.Friendships.Entities;
using GROUPFLOW.Features.Chat.Entities;
using GROUPFLOW.Features.Notifications.Entities;
using GROUPFLOW.Features.Blobs.Entities;

namespace GROUPFLOW.Common.Data;

// Type aliases to resolve namespace conflicts
using PostModel = GROUPFLOW.Features.Posts.Entities.Post;
using FriendshipModel = GROUPFLOW.Features.Friendships.Entities.Friendship;

/// <summary>
/// Handles database seeding with async support and proper logging.
/// </summary>
public class DataInitializer
{
    private readonly AppDbContext _db;
    private readonly ILogger<DataInitializer> _logger;

    public DataInitializer(AppDbContext db, ILogger<DataInitializer> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the database with initial data.
    /// Uses async operations with cancellation token support.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== STARTING DATA SEEDING ===");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await SeedUserRolesAsync(cancellationToken);
            await SeedUsersAsync(cancellationToken);
            await SeedFriendRequestsAsync(cancellationToken);
            await SeedFriendRecommendationsAsync(cancellationToken);
            await SeedProjectsAsync(cancellationToken);
            await SeedUserProjectsAsync(cancellationToken);
            await SeedProjectInvitationsAsync(cancellationToken);
            await SeedPostsAsync(cancellationToken);
            await SeedProjectRecommendationsAsync(cancellationToken);
            await SeedUserChatsAsync(cancellationToken);
            await SeedEntriesAsync(cancellationToken);
            await SeedSharedFilesAsync(cancellationToken);
            await SeedEntryReactionsAsync(cancellationToken);
            await SeedReadBysAsync(cancellationToken);
            await SeedFriendshipsAsync(cancellationToken);
            await SeedProjectEventsAsync(cancellationToken);
            await SeedSavedPostsAsync(cancellationToken);
            await SeedUserSkillsAsync(cancellationToken);
            await SeedUserInterestsAsync(cancellationToken);
            await SeedPostLikesAsync(cancellationToken);
            await SeedPostCommentsAsync(cancellationToken);
            await SeedPostCommentLikesAsync(cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation("=== DATA SEEDING COMPLETED in {ElapsedMs}ms ===", stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Data seeding was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data seeding");
            throw;
        }
    }

    private async Task SeedUserRolesAsync(CancellationToken ct)
    {
        if (await _db.UserRoles.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding User Roles...");
        var roles = new List<UserRole>
        {
            new() { RoleName = AppConstants.RoleUser },
            new() { RoleName = AppConstants.RoleModerator },
            new() { RoleName = AppConstants.RoleAdmin }
        };
        _db.UserRoles.AddRange(roles);
        await _db.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded {Count} user roles", roles.Count);
    }

    private async Task SeedUsersAsync(CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding Users...");
        var users = new List<User>
        {
            new() { Name = "Jan", Surname = "Kowalski", Nickname = "janek", Email = "jan@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=1", Joined = DateTime.UtcNow.AddDays(-10), UserRoleId = 1 },
            new() { Name = "Anna", Surname = "Nowak", Nickname = "ania", Email = "anna@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=2", Joined = DateTime.UtcNow.AddDays(-5), UserRoleId = 1 },
            new() { Name = "Kamil", Surname = "Wójcik", Nickname = "kamil", Email = "kamil@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=3", Joined = DateTime.UtcNow, UserRoleId = 2 },
            new() { Name = "Alice", Surname = "Smith", Nickname = "Alice", Email = "alice@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=4", Joined = DateTime.UtcNow.AddDays(-15), UserRoleId = 1 },
            new() { Name = "Bob", Surname = "Johnson", Nickname = "Bob", Email = "bob@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=5", Joined = DateTime.UtcNow.AddDays(-12), UserRoleId = 1 },
            new() { Name = "Charlie", Surname = "Brown", Nickname = "Charlie", Email = "charlie@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=6", Joined = DateTime.UtcNow.AddDays(-8), UserRoleId = 1 },
            new() { Name = "Eve", Surname = "Williams", Nickname = "Eve", Email = "eve@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=7", Joined = DateTime.UtcNow.AddDays(-6), UserRoleId = 1, IsModerator = true }
        };
        _db.Users.AddRange(users);
        await _db.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded {Count} users", users.Count);
    }

    private async Task SeedFriendRequestsAsync(CancellationToken ct)
    {
        if (await _db.FriendRequests.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding Friend Requests...");
        var requests = new List<FriendRequest>
        {
            new() { RequesterId = 1, RequesteeId = 2, Sent = DateTime.UtcNow.AddDays(-4), Expiring = DateTime.UtcNow.AddDays(2) },
            new() { RequesterId = 2, RequesteeId = 3, Sent = DateTime.UtcNow.AddDays(-3), Expiring = DateTime.UtcNow.AddDays(2) }
        };
        _db.FriendRequests.AddRange(requests);
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedFriendRecommendationsAsync(CancellationToken ct)
    {
        if (await _db.FriendRecommendations.AnyAsync(ct))
            return;

        _db.FriendRecommendations.Add(new FriendRecommendation { RecommendedWhoId = 1, RecommendedForId = 3, RecValue = 1 });
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedProjectsAsync(CancellationToken ct)
    {
        if (await _db.Projects.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding Projects...");
        var projects = new List<Project>
        {
            new()
            {
                Name = "Task Management System",
                Description = "A comprehensive task management application with real-time collaboration",
                OwnerId = 1,
                Image = "https://picsum.photos/600/400?random=1",
                IsPublic = true,
                Created = DateTime.UtcNow.AddDays(-15),
                LastUpdated = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Name = "Weather Dashboard",
                Description = "Beautiful weather dashboard with forecasts and analytics",
                OwnerId = 2,
                Image = "https://picsum.photos/600/400?random=2",
                IsPublic = true,
                Created = DateTime.UtcNow.AddDays(-10),
                LastUpdated = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Name = "Social Media Analytics",
                Description = "Advanced analytics platform for social media insights",
                OwnerId = 3,
                Image = "https://picsum.photos/600/400?random=3",
                IsPublic = true,
                Created = DateTime.UtcNow.AddDays(-8),
                LastUpdated = DateTime.UtcNow.AddHours(-6)
            },
            new()
            {
                Name = "Private Finance Tracker",
                Description = "Personal finance management tool with budgeting features",
                OwnerId = 1,
                Image = "https://picsum.photos/600/400?random=4",
                IsPublic = false,
                Created = DateTime.UtcNow.AddDays(-5),
                LastUpdated = DateTime.UtcNow.AddHours(-12)
            },
            new()
            {
                Name = "E-commerce Platform",
                Description = "Modern e-commerce solution with inventory management",
                OwnerId = 2,
                Image = "https://picsum.photos/600/400?random=5",
                IsPublic = true,
                Created = DateTime.UtcNow.AddDays(-20),
                LastUpdated = DateTime.UtcNow.AddHours(-3)
            },
            new()
            {
                Name = "Fitness Tracker App",
                Description = "Track your workouts and nutrition goals",
                OwnerId = 3,
                Image = "https://picsum.photos/600/400?random=6",
                IsPublic = true,
                Created = DateTime.UtcNow.AddDays(-12),
                LastUpdated = DateTime.UtcNow.AddDays(-1)
            }
        };
        _db.Projects.AddRange(projects);
        await _db.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded {Count} projects", projects.Count);

        // Create chats for projects
        _logger.LogInformation("Seeding Chats for Projects...");
        foreach (var project in projects)
        {
            var chat = new Chat { ProjectId = project.Id };
            _db.Chats.Add(chat);
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedUserProjectsAsync(CancellationToken ct)
    {
        if (await _db.UserProjects.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding User Projects...");
        var userProjects = new List<UserProject>
        {
            new() { UserId = 2, ProjectId = 1, Role = AppConstants.ProjectRoleCollaborator },
            new() { UserId = 3, ProjectId = 2, Role = AppConstants.ProjectRoleAdmin },
            new() { UserId = 1, ProjectId = 3, Role = AppConstants.ProjectRoleViewer }
        };
        _db.UserProjects.AddRange(userProjects);
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedProjectInvitationsAsync(CancellationToken ct)
    {
        if (await _db.ProjectInvitations.AnyAsync(ct))
            return;

        var invitations = new List<ProjectInvitation>
        {
            new() { ProjectId = 1, InvitingId = 1, InvitedId = 3, Sent = DateTime.UtcNow.AddDays(-1), Expiring = DateTime.UtcNow.AddDays(2) }
        };
        _db.ProjectInvitations.AddRange(invitations);
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedPostsAsync(CancellationToken ct)
    {
        if (await _db.Posts.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding Posts...");
        var posts = new List<PostModel>();
        var users = await _db.Users.ToListAsync(ct);
        var projects = await _db.Projects.ToListAsync(ct);

        var alice = users.FirstOrDefault(u => u.Nickname == "Alice");
        var bob = users.FirstOrDefault(u => u.Nickname == "Bob");
        var charlie = users.FirstOrDefault(u => u.Nickname == "Charlie");
        var eve = users.FirstOrDefault(u => u.Nickname == "Eve");

        if (alice != null && projects.Any())
        {
            posts.Add(new PostModel
            {
                UserId = alice.Id,
                ProjectId = projects[0].Id,
                Title = "Saved Post Example",
                Description = "This is a saved post example",
                Content = "This is a saved post example",
                ImageUrl = "https://i.imgur.com/shrimp.png",
                Created = DateTime.UtcNow.AddHours(-2),
                Public = true
            });

            posts.Add(new PostModel
            {
                UserId = alice.Id,
                ProjectId = projects[0].Id,
                Title = "Original Post",
                Description = "hardcoded some test posts for viewing purposes",
                Content = "hardcoded some test posts for viewing purposes",
                ImageUrl = "https://i.imgur.com/shrimp.png",
                Created = DateTime.UtcNow.AddHours(-2),
                Public = true
            });
        }

        if (bob != null && projects.Any())
        {
            posts.Add(new PostModel
            {
                UserId = bob.Id,
                ProjectId = projects[0].Id,
                Title = "Testing Post",
                Description = "Another saved post for testing",
                Content = "Another saved post for testing",
                ImageUrl = null,
                Created = DateTime.UtcNow.AddHours(-5),
                Public = true
            });
        }

        if (charlie != null && projects.Any())
        {
            posts.Add(new PostModel
            {
                UserId = charlie.Id,
                ProjectId = projects[0].Id,
                Title = "Saved Posts Page",
                Description = "Saved posts appear here",
                Content = "Saved posts appear here",
                ImageUrl = "https://picsum.photos/600/300?random=2",
                Created = DateTime.UtcNow.AddDays(-1),
                Public = true
            });
        }

        // Create posts for public projects
        var random = new Random(42); // Fixed seed for deterministic results
        foreach (var project in projects.Where(p => p.IsPublic))
        {
            foreach (var user in users.Where(u => u.Nickname == "janek" || u.Nickname == "ania" || u.Nickname == "kamil"))
            {
                posts.Add(new PostModel
                {
                    UserId = user.Id,
                    ProjectId = project.Id,
                    Title = $"Post by {user.Nickname} in {project.Name}",
                    Description = $"This is a post by {user.Nickname} for project {project.Name}.",
                    Content = $"Content for {user.Nickname} in {project.Name}",
                    ImageUrl = $"https://picsum.photos/600/300?random={user.Id}{project.Id}",
                    Created = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                    Public = true
                });
            }
        }

        _db.Posts.AddRange(posts);
        await _db.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded {Count} posts", posts.Count);

        // Create Eve's shared post
        if (eve != null && projects.Any())
        {
            var aliceOriginalPost = await _db.Posts
                .FirstOrDefaultAsync(p => p.UserId == alice!.Id && p.Content.Contains("hardcoded"), ct);

            if (aliceOriginalPost != null)
            {
                var evePost = new PostModel
                {
                    UserId = eve.Id,
                    ProjectId = projects[0].Id,
                    Title = "Shrimple Post",
                    Description = "Its as shrimple as that",
                    Content = "Its as shrimple as that",
                    ImageUrl = null,
                    SharedPostId = aliceOriginalPost.Id,
                    Created = DateTime.UtcNow.AddHours(-3),
                    Public = true
                };

                _db.Posts.Add(evePost);
                await _db.SaveChangesAsync(ct);
            }
        }
    }

    private async Task SeedProjectRecommendationsAsync(CancellationToken ct)
    {
        if (await _db.ProjectRecommendations.AnyAsync(ct))
            return;

        _db.ProjectRecommendations.Add(new ProjectRecommendation { UserId = 2, ProjectId = 2, RecValue = 2 });
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedUserChatsAsync(CancellationToken ct)
    {
        if (await _db.UserChats.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding UserChats...");
        var chats = await _db.Chats.ToListAsync(ct);
        var projects = await _db.Projects.ToListAsync(ct);
        var userChats = new List<UserChat>();

        foreach (var chat in chats)
        {
            var project = projects.FirstOrDefault(p => p.Id == chat.ProjectId);
            if (project != null)
            {
                userChats.Add(new UserChat { UserId = project.OwnerId, ChatId = chat.Id });

                var collaborators = await _db.UserProjects
                    .Where(up => up.ProjectId == project.Id)
                    .Select(up => up.UserId)
                    .ToListAsync(ct);

                foreach (var collaboratorId in collaborators.Where(c => c != project.OwnerId))
                {
                    userChats.Add(new UserChat { UserId = collaboratorId, ChatId = chat.Id });
                }
            }
        }

        _db.UserChats.AddRange(userChats);
        await _db.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded {Count} user chats", userChats.Count);
    }

    private async Task SeedEntriesAsync(CancellationToken ct)
    {
        if (await _db.Entries.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding Entries...");
        var userChats = await _db.UserChats
            .Include(uc => uc.Chat)
            .Include(uc => uc.User)
            .ToListAsync(ct);

        var project1Chat = await _db.Chats.FirstOrDefaultAsync(c => c.ProjectId == 1, ct);
        if (project1Chat != null)
        {
            var janUserChat = userChats.FirstOrDefault(uc => uc.ChatId == project1Chat.Id && uc.User.Nickname == "janek");
            var annaUserChat = userChats.FirstOrDefault(uc => uc.ChatId == project1Chat.Id && uc.User.Nickname == "ania");

            if (janUserChat != null && annaUserChat != null)
            {
                var entries = new List<Entry>
                {
                    new() { UserChatId = janUserChat.Id, Message = "Cześć wszystkim! Zaczynamy pracę nad nowym projektem", Sent = DateTime.UtcNow.AddDays(-3), Public = true },
                    new() { UserChatId = annaUserChat.Id, Message = "Hej! Super, już się cieszę na współpracę", Sent = DateTime.UtcNow.AddDays(-3).AddHours(2), Public = true },
                    new() { UserChatId = janUserChat.Id, Message = "Potrzebujemy zaprojektować bazę danych. Masz jakieś pomysły?", Sent = DateTime.UtcNow.AddDays(-2), Public = true },
                    new() { UserChatId = annaUserChat.Id, Message = "Myślę że PostgreSQL będzie dobrym wyborem", Sent = DateTime.UtcNow.AddDays(-2).AddHours(3), Public = true },
                    new() { UserChatId = janUserChat.Id, Message = "Zgadzam się! Dodajmy też Redis do cache'owania", Sent = DateTime.UtcNow.AddDays(-1), Public = true },
                    new() { UserChatId = annaUserChat.Id, Message = "Dobry pomysł. Jutro zaczynam implementację API", Sent = DateTime.UtcNow.AddHours(-12), Public = true }
                };
                _db.Entries.AddRange(entries);
                await _db.SaveChangesAsync(ct);
            }
        }
    }

    private async Task SeedSharedFilesAsync(CancellationToken ct)
    {
        if (await _db.SharedFiles.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding SharedFiles...");
        var chats = await _db.Chats.ToListAsync(ct);
        if (chats.Any())
        {
            var files = new List<SharedFile>
            {
                new() { ChatId = chats[0].Id, Link = "/files/project-spec.pdf" },
                new() { ChatId = chats[0].Id, Link = "/files/database-diagram.png" }
            };

            if (chats.Count > 1)
            {
                files.Add(new() { ChatId = chats[1].Id, Link = "/files/api-documentation.md" });
            }

            _db.SharedFiles.AddRange(files);
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task SeedEntryReactionsAsync(CancellationToken ct)
    {
        if (await _db.EntryReactions.AnyAsync(ct))
            return;

        var entries = await _db.Entries.ToListAsync(ct);
        if (entries.Count >= 3)
        {
            var reactions = new List<EntryReaction>
            {
                new() { EntryId = entries[0].Id, UserId = 2, Reaction = "👍" },
                new() { EntryId = entries[2].Id, UserId = 1, Reaction = "❤️" }
            };
            _db.EntryReactions.AddRange(reactions);
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task SeedReadBysAsync(CancellationToken ct)
    {
        if (await _db.ReadBys.AnyAsync(ct))
            return;

        var entries = await _db.Entries.ToListAsync(ct);
        if (entries.Count >= 3)
        {
            var readBys = new List<ReadBy>
            {
                new() { UserId = 1, EntryId = entries[0].Id },
                new() { UserId = 2, EntryId = entries[1].Id },
                new() { UserId = 1, EntryId = entries[2].Id }
            };
            _db.ReadBys.AddRange(readBys);
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task SeedFriendshipsAsync(CancellationToken ct)
    {
        if (await _db.Friendships.AnyAsync(ct))
            return;

        var friendships = new List<FriendshipModel>
        {
            new() { UserId = 1, FriendId = 2, IsAccepted = true, CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new() { UserId = 2, FriendId = 3, IsAccepted = true, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { UserId = 1, FriendId = 3, IsAccepted = false, CreatedAt = DateTime.UtcNow.AddDays(-2) }
        };
        _db.Friendships.AddRange(friendships);
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedProjectEventsAsync(CancellationToken ct)
    {
        if (await _db.ProjectEvents.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding Project Events...");
        var projectEvents = new List<ProjectEvent>
        {
            new() { ProjectId = 1, CreatedById = 1, Title = "Sprint Planning", Description = "Plan tasks for next sprint", EventDate = DateTime.UtcNow.AddDays(3), Time = "10:00", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { ProjectId = 1, CreatedById = 2, Title = "Code Review", Description = "Review PRs from last week", EventDate = DateTime.UtcNow.AddDays(1), Time = "14:00", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { ProjectId = 1, CreatedById = 1, Title = "Deployment", Description = "Deploy v1.2 to production", EventDate = DateTime.UtcNow.AddDays(7), Time = "16:00", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { ProjectId = 2, CreatedById = 2, Title = "API Integration", Description = "Integrate weather API", EventDate = DateTime.UtcNow.AddDays(2), Time = "11:00", CreatedAt = DateTime.UtcNow.AddDays(-4) },
            new() { ProjectId = 3, CreatedById = 3, Title = "Data Pipeline Setup", Description = "Setup data ingestion pipeline", EventDate = DateTime.UtcNow.AddDays(4), Time = "09:00", CreatedAt = DateTime.UtcNow.AddDays(-6) }
        };
        _db.ProjectEvents.AddRange(projectEvents);
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedSavedPostsAsync(CancellationToken ct)
    {
        if (await _db.SavedPosts.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding SavedPosts...");
        var posts = await _db.Posts.Include(p => p.User).ToListAsync(ct);
        var users = await _db.Users.ToListAsync(ct);
        var jan = users.FirstOrDefault(u => u.Nickname == "janek");

        if (jan != null && posts.Count >= 4)
        {
            var savedPosts = new List<SavedPost>();
            var alicePost = posts.FirstOrDefault(p => p.User?.Nickname == "Alice" && p.Content.Contains("saved post example"));
            var bobPost = posts.FirstOrDefault(p => p.User?.Nickname == "Bob");

            if (alicePost != null)
                savedPosts.Add(new() { UserId = jan.Id, PostId = alicePost.Id, SavedAt = DateTime.UtcNow.AddHours(-2) });

            if (bobPost != null)
                savedPosts.Add(new() { UserId = jan.Id, PostId = bobPost.Id, SavedAt = DateTime.UtcNow.AddHours(-5) });

            if (savedPosts.Any())
            {
                _db.SavedPosts.AddRange(savedPosts);
                await _db.SaveChangesAsync(ct);
                _logger.LogDebug("Seeded {Count} saved posts", savedPosts.Count);
            }
        }
    }

    private async Task SeedUserSkillsAsync(CancellationToken ct)
    {
        if (await _db.UserSkills.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding UserSkills...");
        var users = await _db.Users.ToListAsync(ct);

        var skills = new List<UserSkill>
        {
            new() { UserId = users.First(u => u.Nickname == "janek").Id, SkillName = "C#" },
            new() { UserId = users.First(u => u.Nickname == "janek").Id, SkillName = "ASP.NET Core" },
            new() { UserId = users.First(u => u.Nickname == "ania").Id, SkillName = "React" },
            new() { UserId = users.First(u => u.Nickname == "ania").Id, SkillName = "UI/UX" },
            new() { UserId = users.First(u => u.Nickname == "kamil").Id, SkillName = "PostgreSQL" },
            new() { UserId = users.First(u => u.Nickname == "kamil").Id, SkillName = "DevOps" }
        };

        _db.UserSkills.AddRange(skills);
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedUserInterestsAsync(CancellationToken ct)
    {
        if (await _db.UserInterests.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding UserInterests...");
        var users = await _db.Users.ToListAsync(ct);

        var interests = new List<UserInterest>
        {
            new() { UserId = users.First(u => u.Nickname == "janek").Id, InterestName = "Project Management" },
            new() { UserId = users.First(u => u.Nickname == "janek").Id, InterestName = "Startups" },
            new() { UserId = users.First(u => u.Nickname == "ania").Id, InterestName = "Frontend Development" },
            new() { UserId = users.First(u => u.Nickname == "ania").Id, InterestName = "Design Systems" },
            new() { UserId = users.First(u => u.Nickname == "kamil").Id, InterestName = "Data Analytics" },
            new() { UserId = users.First(u => u.Nickname == "kamil").Id, InterestName = "Machine Learning" }
        };

        _db.UserInterests.AddRange(interests);
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedPostLikesAsync(CancellationToken ct)
    {
        if (await _db.PostLikes.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding PostLikes...");
        var users = await _db.Users.ToListAsync(ct);
        var posts = await _db.Posts.ToListAsync(ct);

        if (posts.Any())
        {
            var likes = new List<PostLike>
            {
                new() { UserId = users.First(u => u.Nickname == "janek").Id, PostId = posts[0].Id, CreatedAt = DateTime.UtcNow.AddHours(-3) },
                new() { UserId = users.First(u => u.Nickname == "ania").Id, PostId = posts[0].Id, CreatedAt = DateTime.UtcNow.AddHours(-2) }
            };

            if (posts.Count > 1)
            {
                likes.Add(new() { UserId = users.First(u => u.Nickname == "kamil").Id, PostId = posts[1].Id, CreatedAt = DateTime.UtcNow.AddHours(-1) });
            }

            _db.PostLikes.AddRange(likes);
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task SeedPostCommentsAsync(CancellationToken ct)
    {
        if (await _db.PostComments.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding PostComments...");
        var users = await _db.Users.ToListAsync(ct);
        var posts = await _db.Posts.ToListAsync(ct);

        if (posts.Any())
        {
            var comment1 = new PostComment
            {
                UserId = users.First(u => u.Nickname == "ania").Id,
                PostId = posts[0].Id,
                Content = "Super post! 🔥",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            };

            var comment2 = new PostComment
            {
                UserId = users.First(u => u.Nickname == "kamil").Id,
                PostId = posts[0].Id,
                Content = "Zgadzam się, bardzo przydatne",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            };

            _db.PostComments.AddRange(comment1, comment2);
            await _db.SaveChangesAsync(ct);

            // Reply
            var reply = new PostComment
            {
                UserId = users.First(u => u.Nickname == "janek").Id,
                PostId = posts[0].Id,
                Content = "Dzięki! 💪",
                ParentCommentId = comment1.Id,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            };

            _db.PostComments.Add(reply);
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task SeedPostCommentLikesAsync(CancellationToken ct)
    {
        if (await _db.PostCommentLikes.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding PostCommentLikes...");
        var users = await _db.Users.ToListAsync(ct);
        var comments = await _db.PostComments.ToListAsync(ct);

        if (comments.Any())
        {
            var likes = new List<PostCommentLike>
            {
                new() { UserId = users.First(u => u.Nickname == "janek").Id, PostCommentId = comments[0].Id, CreatedAt = DateTime.UtcNow.AddHours(-1) }
            };

            if (comments.Count > 1)
            {
                likes.Add(new() { UserId = users.First(u => u.Nickname == "ania").Id, PostCommentId = comments[1].Id, CreatedAt = DateTime.UtcNow.AddMinutes(-30) });
            }

            _db.PostCommentLikes.AddRange(likes);
            await _db.SaveChangesAsync(ct);
        }
    }
}
