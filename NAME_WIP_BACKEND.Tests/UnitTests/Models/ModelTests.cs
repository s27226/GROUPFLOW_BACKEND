using NAME_WIP_BACKEND.Models;
using Xunit;

namespace NAME_WIP_BACKEND.Tests.UnitTests.Models;

public class UserModelTests
{
    [Fact]
    public void User_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var user = new User
        {
            Id = 1,
            Name = "John",
            Surname = "Doe",
            Nickname = "johndoe",
            Email = "john@example.com",
            Password = "hashedpassword"
        };

        // Assert
        Assert.Equal(1, user.Id);
        Assert.Equal("John", user.Name);
        Assert.Equal("Doe", user.Surname);
        Assert.Equal("johndoe", user.Nickname);
        Assert.Equal("john@example.com", user.Email);
        Assert.NotNull(user.Posts);
        Assert.NotNull(user.OwnedProjects);
        Assert.NotNull(user.ProjectCollaborations);
    }

    [Fact]
    public void User_CollectionsShouldBeInitialized()
    {
        // Arrange & Act
        var user = new User
        {
            Name = "Test",
            Surname = "User",
            Nickname = "testuser",
            Email = "test@example.com",
            Password = "password"
        };

        // Assert
        Assert.Empty(user.Posts);
        Assert.Empty(user.OwnedProjects);
        Assert.Empty(user.ProjectCollaborations);
        Assert.Empty(user.UserChats);
    }
}

public class PostModelTests
{
    [Fact]
    public void Post_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var post = new Post
        {
            Id = 1,
            Content = "Test content",
            UserId = 1,
            Public = true,
            Title = "Test Title",
            Description = "Test Description",
            ProjectId = 1
        };

        // Assert
        Assert.Equal(1, post.Id);
        Assert.Equal("Test content", post.Content);
        Assert.Equal(1, post.UserId);
        Assert.True(post.Public);
        Assert.Equal("Test Title", post.Title);
        Assert.Equal("Test Description", post.Description);
    }
}

public class ProjectModelTests
{
    [Fact]
    public void Project_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var project = new Project
        {
            Id = 1,
            Name = "Test Project",
            Description = "Test Description",
            OwnerId = 1,
            IsPublic = true
        };

        // Assert
        Assert.Equal(1, project.Id);
        Assert.Equal("Test Project", project.Name);
        Assert.Equal("Test Description", project.Description);
        Assert.Equal(1, project.OwnerId);
        Assert.True(project.IsPublic);
        Assert.NotNull(project.Likes);
        Assert.NotNull(project.Views);
    }

    [Fact]
    public void Project_CollaboratorsShouldBeInitialized()
    {
        // Arrange & Act
        var project = new Project
        {
            Name = "Project",
            Description = "Desc",
            OwnerId = 1
        };

        // Assert
        Assert.NotNull(project.Collaborators);
        Assert.Empty(project.Collaborators);
    }
}

public class FriendshipModelTests
{
    [Fact]
    public void Friendship_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var friendship = new Friendship
        {
            Id = 1,
            UserId = 1,
            FriendId = 2,
            IsAccepted = false
        };

        // Assert
        Assert.Equal(1, friendship.Id);
        Assert.Equal(1, friendship.UserId);
        Assert.Equal(2, friendship.FriendId);
        Assert.False(friendship.IsAccepted);
    }
}

public class FriendRequestModelTests
{
    [Fact]
    public void FriendRequest_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var sent = DateTime.UtcNow;
        var expiring = DateTime.UtcNow.AddDays(7);
        
        var friendRequest = new FriendRequest
        {
            Id = 1,
            RequesterId = 1,
            RequesteeId = 2,
            Sent = sent,
            Expiring = expiring
        };

        // Assert
        Assert.Equal(1, friendRequest.Id);
        Assert.Equal(1, friendRequest.RequesterId);
        Assert.Equal(2, friendRequest.RequesteeId);
        Assert.Equal(sent, friendRequest.Sent);
        Assert.Equal(expiring, friendRequest.Expiring);
    }
}

public class ChatModelTests
{
    [Fact]
    public void Chat_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var chat = new Chat
        {
            Id = 1,
            ProjectId = 1
        };

        // Assert
        Assert.Equal(1, chat.Id);
        Assert.Equal(1, chat.ProjectId);
        Assert.NotNull(chat.UserChats);
        Assert.NotNull(chat.Entries);
        Assert.NotNull(chat.SharedFiles);
    }
}

public class EntryModelTests
{
    [Fact]
    public void Entry_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var sent = DateTime.UtcNow;
        
        var entry = new Entry
        {
            Id = 1,
            UserChatId = 1,
            Message = "Test message",
            Sent = sent,
            Public = true
        };

        // Assert
        Assert.Equal(1, entry.Id);
        Assert.Equal(1, entry.UserChatId);
        Assert.Equal("Test message", entry.Message);
        Assert.Equal(sent, entry.Sent);
        Assert.True(entry.Public);
        Assert.NotNull(entry.Reactions);
        Assert.NotNull(entry.ReadBys);
    }
}
