using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Data;
using GROUPFLOW.Models;
using Xunit;

namespace GROUPFLOW.Tests.IntegrationTests;

public class DatabaseIntegrationTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CanAddAndRetrieveUser()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var user = new User
        {
            Name = "John",
            Surname = "Doe",
            Nickname = "johndoe",
            Email = "john@example.com",
            Password = "hashedpassword",
            Joined = DateTime.UtcNow
        };

        // Act
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var retrievedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "john@example.com");

        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal("John", retrievedUser.Name);
        Assert.Equal("Doe", retrievedUser.Surname);
        Assert.Equal("johndoe", retrievedUser.Nickname);
    }

    [Fact]
    public async Task CanAddAndRetrievePost()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var user = new User
        {
            Name = "Test",
            Surname = "User",
            Nickname = "testuser",
            Email = "test@example.com",
            Password = "password"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var post = new Post
        {
            Title = "Test Post",
            Description = "Test Description",
            Content = "Test post content",
            UserId = user.Id,
            Public = true,
            Created = DateTime.UtcNow
        };

        // Act
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var retrievedPost = await context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Content == "Test post content");

        // Assert
        Assert.NotNull(retrievedPost);
        Assert.Equal("Test post content", retrievedPost.Content);
        Assert.Equal(user.Id, retrievedPost.UserId);
        Assert.NotNull(retrievedPost.User);
        Assert.Equal("testuser", retrievedPost.User.Nickname);
    }

    [Fact]
    public async Task CanAddAndRetrieveProject()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var user = new User
        {
            Name = "Owner",
            Surname = "User",
            Nickname = "owner",
            Email = "owner@example.com",
            Password = "password"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var project = new Project
        {
            Name = "Test Project",
            Description = "A test project",
            OwnerId = user.Id,
            IsPublic = true,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        // Act
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var retrievedProject = await context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Name == "Test Project");

        // Assert
        Assert.NotNull(retrievedProject);
        Assert.Equal("Test Project", retrievedProject.Name);
        Assert.Equal("A test project", retrievedProject.Description);
        Assert.Equal(user.Id, retrievedProject.OwnerId);
        Assert.NotNull(retrievedProject.Owner);
        Assert.Equal("owner", retrievedProject.Owner.Nickname);
    }

    [Fact]
    public async Task CanCreateFriendship()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var user1 = new User { Name = "User", Surname = "One", Nickname = "user1", Email = "user1@example.com", Password = "pass" };
        var user2 = new User { Name = "User", Surname = "Two", Nickname = "user2", Email = "user2@example.com", Password = "pass" };
        
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        var friendship = new Friendship
        {
            UserId = user1.Id,
            FriendId = user2.Id,
            IsAccepted = false,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        context.Friendships.Add(friendship);
        await context.SaveChangesAsync();

        var retrievedFriendship = await context.Friendships
            .Include(f => f.User)
            .Include(f => f.Friend)
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(retrievedFriendship);
        Assert.Equal(user1.Id, retrievedFriendship.UserId);
        Assert.Equal(user2.Id, retrievedFriendship.FriendId);
        Assert.False(retrievedFriendship.IsAccepted);
        Assert.Equal("user1", retrievedFriendship.User.Nickname);
        Assert.Equal("user2", retrievedFriendship.Friend.Nickname);
    }

    [Fact]
    public async Task CanCreateFriendRequest()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var requester = new User { Name = "Req", Surname = "User", Nickname = "requester", Email = "req@example.com", Password = "pass" };
        var requestee = new User { Name = "Rec", Surname = "User", Nickname = "requestee", Email = "rec@example.com", Password = "pass" };
        
        context.Users.AddRange(requester, requestee);
        await context.SaveChangesAsync();

        var friendRequest = new FriendRequest
        {
            RequesterId = requester.Id,
            RequesteeId = requestee.Id,
            Sent = DateTime.UtcNow,
            Expiring = DateTime.UtcNow.AddDays(7)
        };

        // Act
        context.FriendRequests.Add(friendRequest);
        await context.SaveChangesAsync();

        var retrievedRequest = await context.FriendRequests
            .Include(fr => fr.Requester)
            .Include(fr => fr.Requestee)
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(retrievedRequest);
        Assert.Equal(requester.Id, retrievedRequest.RequesterId);
        Assert.Equal(requestee.Id, retrievedRequest.RequesteeId);
        Assert.Equal("requester", retrievedRequest.Requester.Nickname);
        Assert.Equal("requestee", retrievedRequest.Requestee.Nickname);
    }

    [Fact]
    public async Task CanCreateProject()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        
        var user = new User { Name = "Test", Surname = "User", Nickname = "testuser", Email = "test@test.com", Password = "hash" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var project = new Project
        {
            Name = "Test Project",
            Description = "A test project",
            OwnerId = user.Id
        };

        // Act
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var retrievedProject = await context.Projects.FirstOrDefaultAsync(p => p.Name == "Test Project");

        // Assert
        Assert.NotNull(retrievedProject);
        Assert.Equal("Test Project", retrievedProject.Name);
        Assert.Equal("A test project", retrievedProject.Description);
    }

    [Fact]
    public async Task CanCreateUserProjectRelationship()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var user = new User { Name = "Test", Surname = "User", Nickname = "testuser", Email = "test@example.com", Password = "pass" };
        var owner = new User { Name = "Owner", Surname = "User", Nickname = "owner", Email = "owner@example.com", Password = "pass" };
        
        context.Users.AddRange(user, owner);
        await context.SaveChangesAsync();

        var project = new Project
        {
            Name = "Collaborative Project",
            Description = "A project with collaborators",
            OwnerId = owner.Id,
            IsPublic = true
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var userProject = new UserProject
        {
            UserId = user.Id,
            ProjectId = project.Id
        };

        // Act
        context.UserProjects.Add(userProject);
        await context.SaveChangesAsync();

        var retrievedProject = await context.Projects
            .Include(p => p.Collaborators)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == project.Id);

        // Assert
        Assert.NotNull(retrievedProject);
        Assert.Single(retrievedProject.Collaborators);
        Assert.Equal(user.Id, retrievedProject.Collaborators.First().UserId);
        Assert.Equal("testuser", retrievedProject.Collaborators.First().User.Nickname);
    }

    [Fact]
    public async Task CanUpdateProject()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var user = new User { Name = "User", Surname = "Test", Nickname = "user", Email = "user@example.com", Password = "pass" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var project = new Project
        {
            Name = "Original Name",
            Description = "Original Description",
            OwnerId = user.Id
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        // Act
        project.Name = "Updated Name";
        await context.SaveChangesAsync();

        var updatedProject = await context.Projects.FindAsync(project.Id);

        // Assert
        Assert.NotNull(updatedProject);
        Assert.Equal("Updated Name", updatedProject.Name);
        Assert.NotNull(updatedProject.Likes);
        Assert.NotNull(updatedProject.Views);
    }

    [Fact]
    public async Task CanDeletePost()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var user = new User { Name = "User", Surname = "Test", Nickname = "user", Email = "user@example.com", Password = "pass" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var post = new Post
        {
            Title = "Post to delete",
            Description = "This post will be deleted",
            Content = "Post to delete",
            UserId = user.Id,
            Public = true
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Act
        context.Posts.Remove(post);
        await context.SaveChangesAsync();

        var deletedPost = await context.Posts.FindAsync(post.Id);

        // Assert
        Assert.Null(deletedPost);
    }
}
