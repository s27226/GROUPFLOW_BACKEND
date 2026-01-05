using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND;

public class DataInitializer
{
    public static async void Seed(AppDbContext db)
        {
            Console.WriteLine("=== STARTING DATA SEEDING ===");
            
            // === USER ROLES ===
            if (!db.UserRoles.Any())
            {
                Console.WriteLine("Seeding User Roles...");
                var roles = new List<UserRole>
                {
                    new() { RoleName = "User" },
                    new() { RoleName = "Moderator" },
                    new() { RoleName = "Admin" }
                };
                db.UserRoles.AddRange(roles);
                await db.SaveChangesAsync();
            }
            
            // === USERS ===
            if (!db.Users.Any())
            {
                var users = new List<User>
                {
                    new() { Name = "Jan", Surname = "Kowalski", Nickname = "janek", Email = "jan@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=1", Joined = DateTime.UtcNow.AddDays(-10), UserRoleId = 1 },
                    new() { Name = "Anna", Surname = "Nowak", Nickname = "ania", Email = "anna@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=2", Joined = DateTime.UtcNow.AddDays(-5), UserRoleId = 1 },
                    new() { Name = "Kamil", Surname = "Wójcik", Nickname = "kamil", Email = "kamil@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=3", Joined = DateTime.UtcNow, UserRoleId = 2 },
                    new() { Name = "Alice", Surname = "Smith", Nickname = "Alice", Email = "alice@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=4", Joined = DateTime.UtcNow.AddDays(-15), UserRoleId = 1 },
                    new() { Name = "Bob", Surname = "Johnson", Nickname = "Bob", Email = "bob@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=5", Joined = DateTime.UtcNow.AddDays(-12), UserRoleId = 1 },
                    new() { Name = "Charlie", Surname = "Brown", Nickname = "Charlie", Email = "charlie@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=6", Joined = DateTime.UtcNow.AddDays(-8), UserRoleId = 1 },
                    new() { Name = "Eve", Surname = "Williams", Nickname = "Eve", Email = "eve@example.com", Password = BCrypt.Net.BCrypt.HashPassword("123"), ProfilePic = "https://picsum.photos/200/200?random=7", Joined = DateTime.UtcNow.AddDays(-6), UserRoleId = 1, IsModerator = true } // Eve is a moderator
                };
                db.Users.AddRange(users);
                await db.SaveChangesAsync();
            }

            

            // === CHATS ===
            // Chats will be created when projects are created
            // Each project will have its own chat

            // === USERCHATS ===
            // UserChats will be created after projects and chats
            // Based on project collaborators

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
                await db.SaveChangesAsync();
            }

            // === FRIEND REQUESTS ===
            if (!db.FriendRequests.Any())
            {
                var requests = new List<FriendRequest>
                {
                    new() { RequesterId = 1, RequesteeId = 2, Sent = DateTime.UtcNow.AddDays(-4), Expiring = DateTime.UtcNow.AddDays(2) },
                    new() { RequesterId = 2, RequesteeId = 3, Sent = DateTime.UtcNow.AddDays(-3), Expiring = DateTime.UtcNow.AddDays(2) }
                };
                db.FriendRequests.AddRange(requests);
                await db.SaveChangesAsync();
            }

            // === FRIEND RECOMMENDATIONS ===
            if (!db.FriendRecommendations.Any())
            {
                db.FriendRecommendations.Add(new FriendRecommendation { RecommendedWhoId = 1, RecommendedForId = 3, RecValue = 1 });
                await db.SaveChangesAsync();
            }

            // === PROJECTS ===
            if (!db.Projects.Any())
            {
                Console.WriteLine("Seeding Projects...");
                var projects = new List<Project>
                {
                    new()
                    {
                        Name = "Task Management System",
                        Description = "A comprehensive task management application with real-time collaboration",
                        OwnerId = 1, // Jan
                        Image = "https://picsum.photos/600/400?random=1",
                        IsPublic = true,
                        Created = DateTime.UtcNow.AddDays(-15),
                        LastUpdated = DateTime.UtcNow.AddDays(-2)
                    },
                    new()
                    {
                        Name = "Weather Dashboard",
                        Description = "Beautiful weather dashboard with forecasts and analytics",
                        OwnerId = 2, // Anna
                        Image = "https://picsum.photos/600/400?random=2",
                        IsPublic = true,
                        Created = DateTime.UtcNow.AddDays(-10),
                        LastUpdated = DateTime.UtcNow.AddDays(-1)
                    },
                    new()
                    {
                        Name = "Social Media Analytics",
                        Description = "Advanced analytics platform for social media insights",
                        OwnerId = 3, // Kamil
                        Image = "https://picsum.photos/600/400?random=3",
                        IsPublic = true,
                        Created = DateTime.UtcNow.AddDays(-8),
                        LastUpdated = DateTime.UtcNow.AddHours(-6)
                    },
                    new()
                    {
                        Name = "Private Finance Tracker",
                        Description = "Personal finance management tool with budgeting features",
                        OwnerId = 1, // Jan
                        Image = "https://picsum.photos/600/400?random=4",
                        IsPublic = false, // Private project
                        Created = DateTime.UtcNow.AddDays(-5),
                        LastUpdated = DateTime.UtcNow.AddHours(-12)
                    },
                    new()
                    {
                        Name = "E-commerce Platform",
                        Description = "Modern e-commerce solution with inventory management",
                        OwnerId = 2, // Anna
                        Image = "https://picsum.photos/600/400?random=5",
                        IsPublic = true,
                        Created = DateTime.UtcNow.AddDays(-20),
                        LastUpdated = DateTime.UtcNow.AddHours(-3)
                    },
                    new()
                    {
                        Name = "Fitness Tracker App",
                        Description = "Track your workouts and nutrition goals",
                        OwnerId = 3, // Kamil
                        Image = "https://picsum.photos/600/400?random=6",
                        IsPublic = true,
                        Created = DateTime.UtcNow.AddDays(-12),
                        LastUpdated = DateTime.UtcNow.AddDays(-1)
                    }
                };
                db.Projects.AddRange(projects);
                await db.SaveChangesAsync();
                
                // === CHATS (linked to projects) ===
                Console.WriteLine("Seeding Chats for Projects...");
                foreach (var project in projects)
                {
                    var chat = new Chat { ProjectId = project.Id };
                    db.Chats.Add(chat);
                }
                await db.SaveChangesAsync();
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
                await db.SaveChangesAsync();
            }

            // === PROJECT INVITATIONS ===
            if (!db.ProjectInvitations.Any())
            {
                var invitations = new List<ProjectInvitation>
                {
                    new() { ProjectId = 1, InvitingId = 1, InvitedId = 3, Sent = DateTime.UtcNow.AddDays(-1), Expiring = DateTime.UtcNow.AddDays(2) }
                };
                db.ProjectInvitations.AddRange(invitations);
                await db.SaveChangesAsync();
            }

            // === POSTS ===
            if (!db.Posts.Any())
            {
                Console.WriteLine("Seeding Posts...");
                var posts = new List<Post>();
                var users = db.Users.ToList();
                var projects = db.Projects.ToList();

                // Add specific hardcoded posts for saved posts demo
                var alice = users.FirstOrDefault(u => u.Nickname == "Alice");
                var bob = users.FirstOrDefault(u => u.Nickname == "Bob");
                var charlie = users.FirstOrDefault(u => u.Nickname == "Charlie");
                var eve = users.FirstOrDefault(u => u.Nickname == "Eve");

                if (alice != null && projects.Any())
                {
                    posts.Add(new Post
                    {
                        UserId = alice.Id,
                        User = alice,
                        ProjectId = projects[0].Id,
                        Project = projects[0],
                        Title = "Saved Post Example",
                        Description = "This is a saved post example",
                        Content = "This is a saved post example",
                        ImageUrl = "https://i.imgur.com/shrimp.png", // placeholder for shrimp image
                        Created = DateTime.UtcNow.AddHours(-2),
                        Public = true
                    });

                    // Post for sharing
                    posts.Add(new Post
                    {
                        UserId = alice.Id,
                        User = alice,
                        ProjectId = projects[0].Id,
                        Project = projects[0],
                        Title = "Original Post",
                        Description = "hardcoded some test posts for viewing purposes (im gonna make the shrimp the coconut.png of our site)",
                        Content = "hardcoded some test posts for viewing purposes (im gonna make the shrimp the coconut.png of our site)",
                        ImageUrl = "https://i.imgur.com/shrimp.png",
                        Created = DateTime.UtcNow.AddHours(-2),
                        Public = true
                    });
                }

                if (bob != null && projects.Any())
                {
                    posts.Add(new Post
                    {
                        UserId = bob.Id,
                        User = bob,
                        ProjectId = projects[0].Id,
                        Project = projects[0],
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
                    posts.Add(new Post
                    {
                        UserId = charlie.Id,
                        User = charlie,
                        ProjectId = projects[0].Id,
                        Project = projects[0],
                        Title = "Saved Posts Page",
                        Description = "Saved posts appear here",
                        Content = "Saved posts appear here",
                        ImageUrl = "https://picsum.photos/600/300?random=2",
                        Created = DateTime.UtcNow.AddDays(-1),
                        Public = true
                    });
                }

                // Create posts for public projects first
                foreach (var project in projects.Where(p => p.IsPublic))
                {
                    foreach (var user in users.Where(u => u.Nickname == "janek" || u.Nickname == "ania" || u.Nickname == "kamil"))
                    {
                        posts.Add(new Post
                        {
                            UserId = user.Id,
                            User = user,
                            ProjectId = project.Id,
                            Project = project,
                            Title = $"Post by {user.Nickname} in {project.Name}",
                            Description = $"This is a post by {user.Nickname} for project {project.Name}.",
                            Content = $"Content for {user.Nickname} in {project.Name}",
                            ImageUrl = $"https://picsum.photos/600/300?random={user.Id}{project.Id}",
                            Created = DateTime.UtcNow.AddDays(-new Random().Next(1, 30)),
                            Public = true
                        });
                    }
                }

                // Save all posts first so they get IDs
                db.Posts.AddRange(posts);
                await db.SaveChangesAsync();

                // Now create Eve's post that shares Alice's post (after posts have IDs)
                if (eve != null && projects.Any())
                {
                    var aliceOriginalPost = db.Posts.FirstOrDefault(p => p.User.Nickname == "Alice" && p.Content.Contains("hardcoded some test posts"));
                    
                    var evePost = new Post
                    {
                        UserId = eve.Id,
                        User = eve,
                        ProjectId = projects[0].Id,
                        Project = projects[0],
                        Title = "Shrimple Post",
                        Description = "Its as shrimple as that",
                        Content = "Its as shrimple as that",
                        ImageUrl = null,
                        SharedPostId = aliceOriginalPost?.Id,
                        Created = DateTime.UtcNow.AddHours(-3),
                        Public = true
                    };
                    
                    db.Posts.Add(evePost);
                    await db.SaveChangesAsync();
                }
            }

            // === PROJECT RECOMMENDATIONS ===
            if (!db.ProjectRecommendations.Any())
            {
                db.ProjectRecommendations.Add(new ProjectRecommendation { UserId = 2, ProjectId = 2, RecValue = 2 });
                await db.SaveChangesAsync();
            }

            // === USERCHATS (linking users to project chats) ===
            if (!db.UserChats.Any())
            {
                Console.WriteLine("Seeding UserChats...");
                // Get all chats and projects from database
                var chats = db.Chats.ToList();
                var projects = db.Projects.ToList();
                
                // Add project owners and collaborators to their project chats
                var userChats = new List<UserChat>();
                
                foreach (var chat in chats)
                {
                    // Find the project for this chat
                    var project = projects.FirstOrDefault(p => p.Id == chat.ProjectId);
                    if (project != null)
                    {
                        Console.WriteLine($"Adding owner (UserId={project.OwnerId}) to chat {chat.Id} for project {project.Name}");
                        // Add owner to chat
                        userChats.Add(new UserChat { UserId = project.OwnerId, ChatId = chat.Id });
                        
                        // Add all collaborators to chat
                        var collaborators = db.UserProjects
                            .Where(up => up.ProjectId == project.Id)
                            .Select(up => up.UserId)
                            .ToList();
                        
                        foreach (var collaboratorId in collaborators)
                        {
                            // Avoid duplicates if owner is also a collaborator
                            if (collaboratorId != project.OwnerId)
                            {
                                Console.WriteLine($"Adding collaborator (UserId={collaboratorId}) to chat {chat.Id} for project {project.Name}");
                                userChats.Add(new UserChat { UserId = collaboratorId, ChatId = chat.Id });
                            }
                        }
                    }
                }
                
                Console.WriteLine($"Total UserChats to add: {userChats.Count}");
                db.UserChats.AddRange(userChats);
                await db.SaveChangesAsync();
                Console.WriteLine("UserChats seeding completed");
            }

            // === ENTRIES (Chat messages - must come after UserChats) ===
            if (!db.Entries.Any())
            {
                Console.WriteLine("Seeding Entries...");
                var userChats = db.UserChats.Include(uc => uc.Chat).Include(uc => uc.User).ToList();
                
                // Get UserChats for Project 1 (Task Management System)
                var project1Chat = db.Chats.FirstOrDefault(c => c.ProjectId == 1);
                if (project1Chat != null)
                {
                    var janUserChat = userChats.FirstOrDefault(uc => uc.ChatId == project1Chat.Id && uc.User.Nickname == "janek");
                    var annaUserChat = userChats.FirstOrDefault(uc => uc.ChatId == project1Chat.Id && uc.User.Nickname == "ania");
                    
                    if (janUserChat != null && annaUserChat != null)
                    {
                        var entries = new List<Entry>
                        {
                            new() { UserChatId = janUserChat.Id, Message = "Cześć wszystkim! Zaczynamy pracę nad nowym projektem Task Management System", Sent = DateTime.UtcNow.AddDays(-3), Public = true },
                            new() { UserChatId = annaUserChat.Id, Message = "Hej Janek! Super, już się cieszę na współpracę", Sent = DateTime.UtcNow.AddDays(-3).AddHours(2), Public = true },
                            new() { UserChatId = janUserChat.Id, Message = "Potrzebujemy zaprojektować bazę danych. Masz jakieś pomysły?", Sent = DateTime.UtcNow.AddDays(-2), Public = true },
                            new() { UserChatId = annaUserChat.Id, Message = "Myślę że PostgreSQL będzie dobrym wyborem dla tego projektu", Sent = DateTime.UtcNow.AddDays(-2).AddHours(3), Public = true },
                            new() { UserChatId = janUserChat.Id, Message = "Zgadzam się! Dodajmy też Redis do cache'owania", Sent = DateTime.UtcNow.AddDays(-1), Public = true },
                            new() { UserChatId = annaUserChat.Id, Message = "Dobry pomysł. Jutro zaczynam implementację API", Sent = DateTime.UtcNow.AddHours(-12), Public = true },
                            new() { UserChatId = janUserChat.Id, Message = "Właśnie skończyłem mockupy dla UI. Przesyłam na review", Sent = DateTime.UtcNow.AddHours(-8), Public = true },
                            new() { UserChatId = annaUserChat.Id, Message = "Świetnie! Widzę, że design wygląda profesjonalnie. Zaczynam frontend", Sent = DateTime.UtcNow.AddHours(-6), Public = true },
                            new() { UserChatId = janUserChat.Id, Message = "Backend API jest już gotowe na 70%. Zostały jeszcze endpointy do notyfikacji", Sent = DateTime.UtcNow.AddHours(-4), Public = true },
                            new() { UserChatId = annaUserChat.Id, Message = "Super! Mogę Ci pomóc z testami jednostkowymi jak skończę z frontendem", Sent = DateTime.UtcNow.AddHours(-2), Public = true },
                            new() { UserChatId = janUserChat.Id, Message = "To byłoby świetnie! Spotkajmy się jutro na code review", Sent = DateTime.UtcNow.AddHours(-1), Public = true }
                        };
                        db.Entries.AddRange(entries);
                    }
                }
                
                await db.SaveChangesAsync();
            }

            // === SHARED FILES (must come after Chats) ===
            if (!db.SharedFiles.Any())
            {
                Console.WriteLine("Seeding SharedFiles...");
                var chats = db.Chats.ToList();
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
                    
                    db.SharedFiles.AddRange(files);
                    await db.SaveChangesAsync();
                }
            }

            // === ENTRY REACTIONS (must come after Entries) ===
            if (!db.EntryReactions.Any())
            {
                Console.WriteLine("Seeding EntryReactions...");
                var entries = db.Entries.ToList();
                if (entries.Count >= 3)
                {
                    var reactions = new List<EntryReaction>
                    {
                        new() { EntryId = entries[0].Id, UserId = 2, EmoteId = 1 },
                        new() { EntryId = entries[2].Id, UserId = 1, EmoteId = 1 }
                    };
                    db.EntryReactions.AddRange(reactions);
                    await db.SaveChangesAsync();
                }
            }

            // === READ BY (must come after Entries) ===
            if (!db.ReadBys.Any())
            {
                Console.WriteLine("Seeding ReadBy...");
                var entries = db.Entries.ToList();
                if (entries.Count >= 3)
                {
                    var readBys = new List<ReadBy>
                    {
                        new() { UserId = 1, EntryId = entries[0].Id },
                        new() { UserId = 2, EntryId = entries[1].Id },
                        new() { UserId = 1, EntryId = entries[2].Id }
                    };
                    db.ReadBys.AddRange(readBys);
                    await db.SaveChangesAsync();
                }
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
                await db.SaveChangesAsync();
            }

            // === PROJECT EVENTS ===
            if (!db.ProjectEvents.Any())
            {
                var projectEvents = new List<ProjectEvent>
                {
                    // Task Management System (Project 1) - More detailed events
                    new() { ProjectId = 1, CreatedById = 1, Title = "Sprint Planning", Description = "Plan tasks for next sprint and assign story points", EventDate = DateTime.UtcNow.AddDays(3), Time = "10:00", CreatedAt = DateTime.UtcNow.AddDays(-5) },
                    new() { ProjectId = 1, CreatedById = 2, Title = "Code Review", Description = "Review PRs from last week, focus on security and performance", EventDate = DateTime.UtcNow.AddDays(1), Time = "14:00", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                    new() { ProjectId = 1, CreatedById = 1, Title = "Deployment", Description = "Deploy v1.2 to production environment", EventDate = DateTime.UtcNow.AddDays(7), Time = "16:00", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                    new() { ProjectId = 1, CreatedById = 2, Title = "Team Standup", Description = "Daily standup meeting to sync on progress", EventDate = DateTime.UtcNow.AddDays(1), Time = "09:00", CreatedAt = DateTime.UtcNow.AddHours(-12) },
                    new() { ProjectId = 1, CreatedById = 1, Title = "Database Migration", Description = "Run migration scripts for new schema changes", EventDate = DateTime.UtcNow.AddDays(2), Time = "11:30", CreatedAt = DateTime.UtcNow.AddHours(-8) },
                    new() { ProjectId = 1, CreatedById = 2, Title = "UI/UX Workshop", Description = "Discuss improvements to task board interface", EventDate = DateTime.UtcNow.AddDays(4), Time = "13:00", CreatedAt = DateTime.UtcNow.AddHours(-6) },
                    new() { ProjectId = 1, CreatedById = 1, Title = "Performance Testing", Description = "Load testing with 1000 concurrent users", EventDate = DateTime.UtcNow.AddDays(5), Time = "15:00", CreatedAt = DateTime.UtcNow.AddHours(-4) },
                    new() { ProjectId = 1, CreatedById = 2, Title = "Sprint Retrospective", Description = "Review what went well and what can be improved", EventDate = DateTime.UtcNow.AddDays(14), Time = "16:30", CreatedAt = DateTime.UtcNow.AddHours(-2) },
                    
                    // Weather Dashboard (Project 2)
                    new() { ProjectId = 2, CreatedById = 2, Title = "API Integration", Description = "Integrate weather API", EventDate = DateTime.UtcNow.AddDays(2), Time = "11:00", CreatedAt = DateTime.UtcNow.AddDays(-4) },
                    new() { ProjectId = 2, CreatedById = 3, Title = "UI Design Review", Description = "Review dashboard design", EventDate = DateTime.UtcNow.AddDays(5), Time = "15:00", CreatedAt = DateTime.UtcNow.AddDays(-3) },
                    
                    // Social Media Analytics (Project 3)
                    new() { ProjectId = 3, CreatedById = 3, Title = "Data Pipeline Setup", Description = "Setup data ingestion pipeline", EventDate = DateTime.UtcNow.AddDays(4), Time = "09:00", CreatedAt = DateTime.UtcNow.AddDays(-6) },
                    new() { ProjectId = 3, CreatedById = 3, Title = "Testing Phase", Description = "End-to-end testing", EventDate = DateTime.UtcNow.AddDays(10), Time = "13:00", CreatedAt = DateTime.UtcNow.AddHours(-12) }
                };
                db.ProjectEvents.AddRange(projectEvents);
                await db.SaveChangesAsync();
            }

            // === SAVED POSTS ===
            if (!db.SavedPosts.Any())
            {
                Console.WriteLine("Seeding SavedPosts...");
                var posts = db.Posts.ToList();
                var users = db.Users.ToList();
                var jan = users.FirstOrDefault(u => u.Nickname == "janek");
                
                if (jan != null && posts.Count >= 4)
                {
                    // Get the specific posts by Alice, Bob, Charlie, Eve
                    var alicePost = posts.FirstOrDefault(p => p.User.Nickname == "Alice" && p.Content.Contains("saved post example"));
                    var bobPost = posts.FirstOrDefault(p => p.User.Nickname == "Bob");
                    var charliePost = posts.FirstOrDefault(p => p.User.Nickname == "Charlie");
                    var evePost = posts.FirstOrDefault(p => p.User.Nickname == "Eve");
                    
                    var savedPosts = new List<SavedPost>();
                    
                    if (alicePost != null)
                        savedPosts.Add(new() { UserId = jan.Id, PostId = alicePost.Id, SavedAt = DateTime.UtcNow.AddHours(-2) });
                    
                    if (bobPost != null)
                        savedPosts.Add(new() { UserId = jan.Id, PostId = bobPost.Id, SavedAt = DateTime.UtcNow.AddHours(-5) });
                    
                    if (charliePost != null)
                        savedPosts.Add(new() { UserId = jan.Id, PostId = charliePost.Id, SavedAt = DateTime.UtcNow.AddDays(-1) });
                    
                    if (evePost != null)
                        savedPosts.Add(new() { UserId = jan.Id, PostId = evePost.Id, SavedAt = DateTime.UtcNow.AddHours(-3) });
                    
                    // Anna (UserId=2) saves some posts too
                    var anna = users.FirstOrDefault(u => u.Nickname == "ania");
                    if (anna != null && posts.Count >= 5)
                    {
                        savedPosts.Add(new() { UserId = anna.Id, PostId = posts[4].Id, SavedAt = DateTime.UtcNow.AddDays(-3) });
                        savedPosts.Add(new() { UserId = anna.Id, PostId = posts[5].Id, SavedAt = DateTime.UtcNow.AddHours(-10) });
                    }
                    
                    db.SavedPosts.AddRange(savedPosts);
                    await db.SaveChangesAsync();
                    Console.WriteLine($"Seeded {savedPosts.Count} saved posts");
                }
            }

            // === USER SKILLS ===
            if (!db.UserSkills.Any())
            {
                Console.WriteLine("Seeding UserSkills...");
                var users = db.Users.ToList();

                var skills = new List<UserSkill>
                {
                    new() { UserId = users.First(u => u.Nickname == "janek").Id, SkillName = "C#" },
                    new() { UserId = users.First(u => u.Nickname == "janek").Id, SkillName = "ASP.NET Core" },

                    new() { UserId = users.First(u => u.Nickname == "ania").Id, SkillName = "React" },
                    new() { UserId = users.First(u => u.Nickname == "ania").Id, SkillName = "UI/UX" },

                    new() { UserId = users.First(u => u.Nickname == "kamil").Id, SkillName = "PostgreSQL" },
                    new() { UserId = users.First(u => u.Nickname == "kamil").Id, SkillName = "DevOps" },
                    
                    new() { UserId = users.First(u => u.Nickname == "Alice").Id, SkillName = "PostgreSQL" },
                    new() { UserId = users.First(u => u.Nickname == "Alice").Id, SkillName = "UI/UX" },
                    
                    new() { UserId = users.First(u => u.Nickname == "Bob").Id, SkillName = "ASP.NET Core" },
                    new() { UserId = users.First(u => u.Nickname == "Bob").Id, SkillName = "DevOps" },
                    
                    new() { UserId = users.First(u => u.Nickname == "Charlie").Id, SkillName = "PostgreSQL" },
                    new() { UserId = users.First(u => u.Nickname == "Charlie").Id, SkillName = "DevOps" },
                    
                    new() { UserId = users.First(u => u.Nickname == "Eve").Id, SkillName = "MySQL" },
                    new() { UserId = users.First(u => u.Nickname == "Eve").Id, SkillName = "C#" }
                };

                db.UserSkills.AddRange(skills);
                await db.SaveChangesAsync();
            }
            
            // === USER INTERESTS ===
            if (!db.UserInterests.Any())
            {
                Console.WriteLine("Seeding UserInterests...");
                var users = db.Users.ToList();

                var interests = new List<UserInterest>
                {
                    new() { UserId = users.First(u => u.Nickname == "janek").Id, InterestName = "Project Management" },
                    new() { UserId = users.First(u => u.Nickname == "janek").Id, InterestName = "Startups" },

                    new() { UserId = users.First(u => u.Nickname == "ania").Id, InterestName = "Frontend Development" },
                    new() { UserId = users.First(u => u.Nickname == "ania").Id, InterestName = "Design Systems" },

                    new() { UserId = users.First(u => u.Nickname == "kamil").Id, InterestName = "Data Analytics" },
                    new() { UserId = users.First(u => u.Nickname == "kamil").Id, InterestName = "Machine Learning" },
                    
                    new() { UserId = users.First(u => u.Nickname == "Alice").Id, InterestName = "Machine Learning" },
                    new() { UserId = users.First(u => u.Nickname == "Alice").Id, InterestName = "Project Management" },
                    
                    new() { UserId = users.First(u => u.Nickname == "Bob").Id, InterestName = "Machine Learning" },
                    new() { UserId = users.First(u => u.Nickname == "Bob").Id, InterestName = "Reading" },
                    
                    new() { UserId = users.First(u => u.Nickname == "Charlie").Id, InterestName = "Machine Learning" },
                    new() { UserId = users.First(u => u.Nickname == "Charlie").Id, InterestName = "Design Systems" },
                    
                    new() { UserId = users.First(u => u.Nickname == "Eve").Id, InterestName = "Reading" },
                    new() { UserId = users.First(u => u.Nickname == "Eve").Id, InterestName = "Data Analytics" }
                };

                db.UserInterests.AddRange(interests);
                await db.SaveChangesAsync();
            }
            
            // === POST LIKES ===
            if (!db.PostLikes.Any())
            {
                Console.WriteLine("Seeding PostLikes...");
                var users = db.Users.ToList();
                var posts = db.Posts.ToList();

                if (posts.Any())
                {
                    var likes = new List<PostLike>
                    {
                        new()
                        {
                            UserId = users.First(u => u.Nickname == "janek").Id,
                            PostId = posts[0].Id,
                            CreatedAt = DateTime.UtcNow.AddHours(-3)
                        },
                        new()
                        {
                            UserId = users.First(u => u.Nickname == "ania").Id,
                            PostId = posts[0].Id,
                            CreatedAt = DateTime.UtcNow.AddHours(-2)
                        },
                        new()
                        {
                            UserId = users.First(u => u.Nickname == "kamil").Id,
                            PostId = posts[1].Id,
                            CreatedAt = DateTime.UtcNow.AddHours(-1)
                        },
                        new()
                        {
                            UserId = users.First(u => u.Nickname == "ania").Id,
                            PostId = posts[1].Id,
                            CreatedAt = DateTime.UtcNow.AddHours(-1)
                        }
                        ,
                        new()
                        {
                            UserId = users.First(u => u.Nickname == "kamil").Id,
                            PostId = posts[2].Id,
                            CreatedAt = DateTime.UtcNow.AddHours(-1)
                        }
                    };

                    db.PostLikes.AddRange(likes);
                    await db.SaveChangesAsync();
                }
            }
            
            // === POST COMMENTS ===
            if (!db.PostComments.Any())
            {
                Console.WriteLine("Seeding PostComments...");
                var users = db.Users.ToList();
                var posts = db.Posts.ToList();

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

                    db.PostComments.AddRange(comment1, comment2);
                    await db.SaveChangesAsync();

                    // Reply
                    var reply = new PostComment
                    {
                        UserId = users.First(u => u.Nickname == "janek").Id,
                        PostId = posts[0].Id,
                        Content = "Dzięki! 💪",
                        ParentCommentId = comment1.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(-2)
                    };

                    db.PostComments.Add(reply);
                    await db.SaveChangesAsync();
                }
            }
            
            // === POST COMMENT LIKES ===
            if (!db.PostCommentLikes.Any())
            {
                Console.WriteLine("Seeding PostCommentLikes...");
                var users = db.Users.ToList();
                var comments = db.PostComments.ToList();

                if (comments.Any())
                {
                    var likes = new List<PostCommentLike>
                    {
                        new()
                        {
                            UserId = users.First(u => u.Nickname == "janek").Id,
                            PostCommentId = comments[0].Id,
                            CreatedAt = DateTime.UtcNow.AddHours(-1)
                        },
                        new()
                        {
                            UserId = users.First(u => u.Nickname == "ania").Id,
                            PostCommentId = comments[1].Id,
                            CreatedAt = DateTime.UtcNow.AddMinutes(-30)
                        }
                        ,
                        new()
                        {
                            UserId = users.First(u => u.Nickname == "kamil").Id,
                            PostCommentId = comments[1].Id,
                            CreatedAt = DateTime.UtcNow.AddMinutes(-30)
                        },
                        new()
                        {
                            UserId = users.First(u => u.Nickname == "ania").Id,
                            PostCommentId = comments[2].Id,
                            CreatedAt = DateTime.UtcNow.AddMinutes(-30)
                        }
                    };

                    db.PostCommentLikes.AddRange(likes);
                    await db.SaveChangesAsync();
                }
            }
            
            
            
            
            
            
            
            

        }
}