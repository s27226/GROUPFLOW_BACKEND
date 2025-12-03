using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND;

public class DataInitializer
{
    public static void Seed(AppDbContext db)
        {
            db.Database.EnsureCreated();

            // === USER ROLES ===
            if (!db.UserRoles.Any())
            {
                var roles = new List<UserRole>
                {
                    new() { RoleName = "User" },
                    new() { RoleName = "Moderator" },
                    new() { RoleName = "Admin" }
                };
                db.UserRoles.AddRange(roles);
                db.SaveChanges();
            }
            
            // === GROUPS ===
            if (!db.Groups.Any())
            {
                var groups = new List<Group>
                {
                    new() { Name = "Developers", Desc = "Grupa dla programistów"},
                    new() { Name = "Sport Team", Desc = "Miłośnicy sportu" }
                };
                db.Groups.AddRange(groups);
                db.SaveChanges();
            }
            
            // === USERS ===
            if (!db.Users.Any())
            {
                var users = new List<User>
                {
                    new() { Name = "Jan", Surname = "Kowalski", Nickname = "janek", Email = "jan@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), Joined = DateTime.UtcNow.AddDays(-10), UserRoleId = 1 },
                    new() { Name = "Anna", Surname = "Nowak", Nickname = "ania", Email = "anna@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), Joined = DateTime.UtcNow.AddDays(-5), UserRoleId = 1 },
                    new() { Name = "Kamil", Surname = "Wójcik", Nickname = "kamil", Email = "kamil@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), Joined = DateTime.UtcNow, UserRoleId = 2 }
                };
                db.Users.AddRange(users);
                db.SaveChanges();
            }

            

            // === USERGROUPS ===
            if (!db.UserGroups.Any())
            {
                var userGroups = new List<UserGroup>
                {
                    new() { UserId = 1, GroupId = 1 },
                    new() { UserId = 2, GroupId = 1 },
                    new() { UserId = 3, GroupId = 2 }
                };
                db.UserGroups.AddRange(userGroups);
                db.SaveChanges();
            }

            // === CHATS ===
            if (!db.Chats.Any())
            {
                var chats = new List<Chat>
                {
                    new() { GroupId = 1},
                    new() { GroupId = 2}
                };
                db.Chats.AddRange(chats);
                db.SaveChanges();
            }

            // === USERCHATS ===
            if (!db.UserChats.Any())
            {
                var userChats = new List<UserChat>
                {
                    new() { UserId = 1, ChatId = 1 },
                    new() { UserId = 2, ChatId = 1 },
                    new() { UserId = 3, ChatId = 2 }
                };
                db.UserChats.AddRange(userChats);
                db.SaveChanges();
            }

            // === EMOTES ===
            if (!db.Emotes.Any())
            {
                var emotes = new List<Emote>
                {
                    new() { Name = "like"},
                    new() { Name = "Hate"},
                    new() { Name = "Eh"}
                };
                db.Emotes.AddRange(emotes);
                db.SaveChanges();
            }

            // === ENTRIES ===
            if (!db.Entries.Any())
            {
                var entries = new List<Entry>
                {
                    new() {  UserChatId =  1, Message = "Cześć wszystkim!", Sent = DateTime.UtcNow.AddDays(-3),Public = true},
                    new() {  UserChatId = 1, Message = "Hej Janek!", Sent = DateTime.UtcNow.AddDays(-2),Public = true },
                    new() {  UserChatId = 2, Message = "Witajcie devy!", Sent = DateTime.UtcNow.AddDays(-1),Public = true }
                };
                db.Entries.AddRange(entries);
                db.SaveChanges();
            }

            // === ENTRY REACTIONS ===
            if (!db.EntryReactions.Any())
            {
                var reactions = new List<EntryReaction>
                {
                    new() { EntryId = 1, UserId = 2, EmoteId = 1 },
                    new() { EntryId = 3, UserId = 1, EmoteId = 2 }
                };
                db.EntryReactions.AddRange(reactions);
                db.SaveChanges();
            }

            // === FRIEND REQUESTS ===
            if (!db.FriendRequests.Any())
            {
                var requests = new List<FriendRequest>
                {
                    new() { RequesterId = 1, RequesteeId = 2, Sent = DateTime.UtcNow.AddDays(-4), Expiring =DateTime.UtcNow.AddDays(2) },
                    new() { RequesterId = 2, RequesteeId = 3, Sent = DateTime.UtcNow.AddDays(-3), Expiring =DateTime.UtcNow.AddDays(2) }
                };
                db.FriendRequests.AddRange(requests);
                db.SaveChanges();
            }

            // === GROUP INVITATIONS ===
            if (!db.GroupInvitations.Any())
            {
                var invitations = new List<GroupInvitation>
                {
                    new() { GroupId = 1, InvitingId = 1, InvitedId = 3, Sent = DateTime.UtcNow.AddDays(-1),Expiring  =DateTime.UtcNow.AddDays(2) }
                };
                db.GroupInvitations.AddRange(invitations);
                db.SaveChanges();
            }

            // === SHARED FILES ===
            if (!db.SharedFiles.Any())
            {
                var files = new List<SharedFile>
                {
                    new() { ChatId = 1, Link = "/files/readme.txt" },
                    new() { ChatId = 2, Link = "/files/logo.png" }
                };
                db.SharedFiles.AddRange(files);
                db.SaveChanges();
            }

            // === READ BY ===
            if (!db.ReadBys.Any())
            {
                var read = new List<ReadBy>
                {
                    new() { UserId = 1, EntryId = 1 },
                    new() { UserId = 2, EntryId = 2 },
                    new() { UserId = 3, EntryId = 3 }
                };
                db.ReadBys.AddRange(read);
                db.SaveChanges();
            }

            // === RECOMMENDATIONS ===
            if (!db.FriendRecommendations.Any())
            {
                db.FriendRecommendations.Add(new FriendRecommendation { RecommendedWhoId = 1, RecommendedForId  = 3, RecValue = 1 });
                
                db.SaveChanges();
            }
            
            
            if (!db.GroupRecommendations.Any())
            {
                
                db.GroupRecommendations.Add(new GroupRecommendation { UserId = 2, GroupId = 2, RecValue = 2 });
                db.SaveChanges();
            }

            // === POSTS ===
            if (!db.Posts.Any())
            {
                var posts = new List<Post>();
                var users = db.Users.ToList();
                var groups = db.Groups.ToList();

                foreach (var user in users)
                {
                    foreach (var group in groups)
                    {
                        posts.Add(new Post
                        {
                            UserId = user.Id,
                            User = user,
                            GroupId = group.Id,
                            Group = group,
                            Title = $"Post by {user.Nickname} in {group.Name}",
                            Description = $"This is a post by {user.Nickname} for group {group.Name}.",
                            Content = $"Content for {user.Nickname} in {group.Name}",
                            ImageUrl = "https://picsum.photos/600/300",
                            Created = DateTime.UtcNow,
                            Public = true
                        });
                    }
                }

                db.Posts.AddRange(posts);
                db.SaveChanges();
            }

            // === PROJECTS ===
            if (!db.Projects.Any())
            {
                var projects = new List<Project>
                {
                    new()
                    {
                        Name = "Task Management System",
                        Description = "A comprehensive task management application with real-time collaboration",
                        OwnerId = 1, // Jan
                        ImageUrl = "https://picsum.photos/600/400?random=1",
                        IsPublic = true,
                        ViewCount = 125,
                        LikeCount = 23,
                        Created = DateTime.UtcNow.AddDays(-15),
                        LastUpdated = DateTime.UtcNow.AddDays(-2)
                    },
                    new()
                    {
                        Name = "Weather Dashboard",
                        Description = "Beautiful weather dashboard with forecasts and analytics",
                        OwnerId = 2, // Anna
                        ImageUrl = "https://picsum.photos/600/400?random=2",
                        IsPublic = true,
                        ViewCount = 89,
                        LikeCount = 34,
                        Created = DateTime.UtcNow.AddDays(-10),
                        LastUpdated = DateTime.UtcNow.AddDays(-1)
                    },
                    new()
                    {
                        Name = "Social Media Analytics",
                        Description = "Advanced analytics platform for social media insights",
                        OwnerId = 3, // Kamil
                        ImageUrl = "https://picsum.photos/600/400?random=3",
                        IsPublic = true,
                        ViewCount = 256,
                        LikeCount = 67,
                        Created = DateTime.UtcNow.AddDays(-8),
                        LastUpdated = DateTime.UtcNow.AddHours(-6)
                    },
                    new()
                    {
                        Name = "Private Finance Tracker",
                        Description = "Personal finance management tool with budgeting features",
                        OwnerId = 1, // Jan
                        ImageUrl = "https://picsum.photos/600/400?random=4",
                        IsPublic = false, // Private project
                        ViewCount = 12,
                        LikeCount = 3,
                        Created = DateTime.UtcNow.AddDays(-5),
                        LastUpdated = DateTime.UtcNow.AddHours(-12)
                    }
                };
                db.Projects.AddRange(projects);
                db.SaveChanges();
            }

            // === USER PROJECTS (Collaborations) ===
            if (!db.UserProjects.Any())
            {
                var userProjects = new List<UserProject>
                {
                    new() { UserId = 2, ProjectId = 1, Role = "Collaborator" }, // Anna collaborates on Jan's task manager
                    new() { UserId = 3, ProjectId = 2, Role = "Admin" }, // Kamil is admin on Anna's weather dashboard
                    new() { UserId = 1, ProjectId = 3, Role = "Viewer" } // Jan can view Kamil's analytics platform
                };
                db.UserProjects.AddRange(userProjects);
                db.SaveChanges();
            }

            // === FRIENDSHIPS ===
            if (!db.Friendships.Any())
            {
                var friendships = new List<Friendship>
                {
                    new() { UserId = 1, FriendId = 2, IsAccepted = true, CreatedAt = DateTime.UtcNow.AddDays(-7) }, // Jan and Anna are friends
                    new() { UserId = 2, FriendId = 3, IsAccepted = true, CreatedAt = DateTime.UtcNow.AddDays(-5) }, // Anna and Kamil are friends
                    new() { UserId = 1, FriendId = 3, IsAccepted = false, CreatedAt = DateTime.UtcNow.AddDays(-2) } // Jan sent friend request to Kamil (pending)
                };
                db.Friendships.AddRange(friendships);
                db.SaveChanges();
            }


        }
}