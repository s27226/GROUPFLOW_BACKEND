using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Users.Entities;
using GROUPFLOW.Features.Users.GraphQL.Queries;
using Xunit;

namespace GROUPFLOW.Tests.UnitTests.Queries;

/// <summary>
/// Unit tests for Users GraphQL queries
/// </summary>
public class UsersQueryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UsersQuery _usersQuery;

    public UsersQueryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _usersQuery = new UsersQuery();
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        var users = new[]
        {
            new User
            {
                Id = 1,
                Name = "Alice",
                Surname = "Johnson",
                Nickname = "alice",
                Email = "alice@example.com",
                Password = "hashed1"
            },
            new User
            {
                Id = 2,
                Name = "Bob",
                Surname = "Smith",
                Nickname = "bob",
                Email = "bob@example.com",
                Password = "hashed2"
            },
            new User
            {
                Id = 3,
                Name = "Charlie",
                Surname = "Brown",
                Nickname = "charlie",
                Email = "charlie@example.com",
                Password = "hashed3"
            }
        };

        _context.Users.AddRange(users);
        _context.SaveChanges();
    }

    [Fact]
    public void GetUsers_ShouldReturnAllUsers()
    {
        // Act
        var result = _usersQuery.GetUsers(_context).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(u => u.Name == "Alice");
        result.Should().Contain(u => u.Name == "Bob");
        result.Should().Contain(u => u.Name == "Charlie");
    }

    [Fact]
    public void GetUsers_ShouldReturnQueryable()
    {
        // Act
        var result = _usersQuery.GetUsers(_context);

        // Assert
        result.Should().BeAssignableTo<IQueryable<User>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetUserById_WithValidId_ShouldReturnUser()
    {
        // Act
        var result = _usersQuery.GetUserById(_context, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Alice");
        result.Surname.Should().Be("Johnson");
        result.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public void GetUserById_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = _usersQuery.GetUserById(_context, 999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserById_WithZeroId_ShouldReturnNull()
    {
        // Act
        var result = _usersQuery.GetUserById(_context, 0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUsers_WithEmptyDatabase_ShouldReturnEmptyQueryable()
    {
        // Arrange
        _context.Users.RemoveRange(_context.Users);
        _context.SaveChanges();

        // Act
        var result = _usersQuery.GetUsers(_context).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetUsers_SupportsLinqOperations()
    {
        // Act
        var result = _usersQuery.GetUsers(_context)
            .Where(u => u.Name.StartsWith("A"))
            .OrderBy(u => u.Name)
            .ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Alice");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
