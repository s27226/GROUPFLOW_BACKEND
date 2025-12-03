using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using Xunit;

namespace NAME_WIP_BACKEND.Tests.IntegrationTests;

/// <summary>
/// Integration tests for database operations and Entity Framework relationships
/// </summary>
public class DatabaseTests : IDisposable
{
    private readonly AppDbContext _context;

    public DatabaseTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Database_CanSaveAndRetrieveUser()
    {
        // Arrange
        var user = new User
        {
            Name = "Integration",
            Surname = "Test",
            Nickname = "integtest",
            Email = "integration@test.com",
            Password = "hashedPassword"
        };

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == "integration@test.com");
        
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Name.Should().Be("Integration");
        retrievedUser.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Database_CanSaveUserWithPosts()
    {
        // Arrange
        var user = new User
        {
            Name = "Post",
            Surname = "Author",
            Nickname = "postauthor",
            Email = "author@test.com",
            Password = "hashedPassword"
        };

        var post = new Post
        {
            Title = "Test Post",
            Content = "Test Content",
            Description = "Test Description",
            User = user
        };

        // Act
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedUser = await _context.Users
            .Include(u => u.Posts)
            .FirstOrDefaultAsync(u => u.Email == "author@test.com");

        retrievedUser.Should().NotBeNull();
        retrievedUser!.Posts.Should().HaveCount(1);
        retrievedUser.Posts.First().Title.Should().Be("Test Post");
    }

    [Fact]
    public async Task Database_CanSaveUserWithProjects()
    {
        // Arrange
        var owner = new User
        {
            Name = "Project",
            Surname = "Owner",
            Nickname = "projectowner",
            Email = "owner@test.com",
            Password = "hashedPassword"
        };

        var project = new Project
        {
            Name = "Test Project",
            Description = "Project Description",
            Owner = owner
        };

        // Act
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedUser = await _context.Users
            .Include(u => u.OwnedProjects)
            .FirstOrDefaultAsync(u => u.Email == "owner@test.com");

        retrievedUser.Should().NotBeNull();
        retrievedUser!.OwnedProjects.Should().HaveCount(1);
        retrievedUser.OwnedProjects.First().Name.Should().Be("Test Project");
    }

    [Fact]
    public async Task Database_CanQueryMultipleUsers()
    {
        // Arrange
        var users = new[]
        {
            new User { Name = "User1", Surname = "Test1", Nickname = "user1", Email = "user1@test.com", Password = "hash1" },
            new User { Name = "User2", Surname = "Test2", Nickname = "user2", Email = "user2@test.com", Password = "hash2" },
            new User { Name = "User3", Surname = "Test3", Nickname = "user3", Email = "user3@test.com", Password = "hash3" }
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        // Act
        var count = await _context.Users.CountAsync();
        var userList = await _context.Users.ToListAsync();

        // Assert
        count.Should().Be(3);
        userList.Should().HaveCount(3);
    }

    [Fact]
    public async Task Database_CanUpdateUser()
    {
        // Arrange
        var user = new User
        {
            Name = "Original",
            Surname = "Name",
            Nickname = "original",
            Email = "original@test.com",
            Password = "hashedPassword"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        user.Name = "Updated";
        await _context.SaveChangesAsync();

        // Assert
        var updatedUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == "original@test.com");
        
        updatedUser.Should().NotBeNull();
        updatedUser!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task Database_CanDeleteUser()
    {
        // Arrange
        var user = new User
        {
            Name = "Delete",
            Surname = "Me",
            Nickname = "deleteme",
            Email = "delete@test.com",
            Password = "hashedPassword"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Assert
        var deletedUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == "delete@test.com");
        
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task Database_SupportsComplexQueries()
    {
        // Arrange
        var users = new[]
        {
            new User { Name = "Alpha", Surname = "User", Nickname = "alpha", Email = "alpha@test.com", Password = "hash" },
            new User { Name = "Beta", Surname = "User", Nickname = "beta", Email = "beta@test.com", Password = "hash" },
            new User { Name = "Alpha", Surname = "Another", Nickname = "alpha2", Email = "alpha2@test.com", Password = "hash" }
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _context.Users
            .Where(u => u.Name == "Alpha")
            .OrderBy(u => u.Surname)
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Surname.Should().Be("Another");
        result.Last().Surname.Should().Be("User");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
