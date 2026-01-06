using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.Services;
using System.Security.Claims;
using Xunit;

namespace NAME_WIP_BACKEND.Tests.UnitTests.Services;

/// <summary>
/// Unit tests for ProjectService business logic
/// </summary>
public class ProjectServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<ProjectService>> _loggerMock;
    private readonly ProjectService _projectService;
    private readonly ClaimsPrincipal _testUser;

    public ProjectServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new AppDbContext(options);
        _loggerMock = new Mock<ILogger<ProjectService>>();
        _projectService = new ProjectService(_context, _loggerMock.Object);

        // Create test user claims
        _testUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }));

        SeedTestData();
    }

    private void SeedTestData()
    {
        var user = new User
        {
            Id = 1,
            Name = "Test",
            Surname = "User",
            Nickname = "testuser",
            Email = "test@example.com",
            Password = "hashedpassword"
        };

        _context.Users.Add(user);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateProject_WithValidInput_ShouldCreateProjectSuccessfully()
    {
        // Arrange
        var input = new ProjectInput(
            Name: "Test Project",
            Description: "Test Description",
            ImageUrl: "https://example.com/image.jpg",
            IsPublic: true,
            Skills: new[] { "C#", "ASP.NET" },
            Interests: new[] { "Web Development" }
        );

        // Act
        var result = await _projectService.CreateProject(_testUser, input);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Project");
        result.Description.Should().Be("Test Description");
        result.Image.Should().Be("https://example.com/image.jpg");
        result.IsPublic.Should().BeTrue();
        result.OwnerId.Should().Be(1);

        // Verify project was saved
        var savedProject = await _context.Projects.FindAsync(result.Id);
        savedProject.Should().NotBeNull();

        // Verify skills and interests were added
        var projectSkills = await _context.ProjectSkills
            .Where(ps => ps.ProjectId == result.Id)
            .Select(ps => ps.SkillName)
            .ToListAsync();
        projectSkills.Should().BeEquivalentTo(new[] { "C#", "ASP.NET" });

        var projectInterests = await _context.ProjectInterests
            .Where(pi => pi.ProjectId == result.Id)
            .Select(pi => pi.InterestName)
            .ToListAsync();
        projectInterests.Should().BeEquivalentTo(new[] { "Web Development" });

        // Verify chat was created
        var chat = await _context.Chats.FirstOrDefaultAsync(c => c.ProjectId == result.Id);
        chat.Should().NotBeNull();

        // Verify owner was added to chat
        var userChat = await _context.UserChats
            .FirstOrDefaultAsync(uc => uc.ChatId == chat.Id && uc.UserId == 1);
        userChat.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProject_WithEmptySkillsAndInterests_ShouldCreateProjectSuccessfully()
    {
        // Arrange
        var input = new ProjectInput(
            Name: "Simple Project",
            Description: "Simple Description",
            ImageUrl: null,
            IsPublic: false,
            Skills: Array.Empty<string>(),
            Interests: Array.Empty<string>()
        );

        // Act
        var result = await _projectService.CreateProject(_testUser, input);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Simple Project");
        result.IsPublic.Should().BeFalse();

        // Verify no skills or interests were added
        var projectSkills = await _context.ProjectSkills
            .Where(ps => ps.ProjectId == result.Id)
            .ToListAsync();
        projectSkills.Should().BeEmpty();

        var projectInterests = await _context.ProjectInterests
            .Where(pi => pi.ProjectId == result.Id)
            .ToListAsync();
        projectInterests.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateProject_WithValidInput_ShouldUpdateProjectSuccessfully()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Original Name",
            Description = "Original Description",
            Image = "original.jpg",
            IsPublic = false,
            OwnerId = 1,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var updateInput = new UpdateProjectInput(
            Id: existingProject.Id,
            Name: "Updated Name",
            Description: "Updated Description",
            ImageUrl: "updated.jpg",
            IsPublic: true
        );

        // Act
        var result = await _projectService.UpdateProject(_testUser, updateInput);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.Image.Should().Be("updated.jpg");
        result.IsPublic.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProject_WithNonExistentProject_ShouldReturnNull()
    {
        // Arrange
        var updateInput = new UpdateProjectInput(
            Id: 999,
            Name: "Updated Name",
            Description: null,
            ImageUrl: null,
            IsPublic: null
        );

        // Act
        var result = await _projectService.UpdateProject(_testUser, updateInput);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProject_WithUnauthorizedUser_ShouldThrowException()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Original Name",
            Description = "Original Description",
            OwnerId = 2, // Different owner
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var updateInput = new UpdateProjectInput(
            Id: existingProject.Id,
            Name: "Updated Name",
            Description: null,
            ImageUrl: null,
            IsPublic: null
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HotChocolate.GraphQLException>(
            () => _projectService.UpdateProject(_testUser, updateInput));

        exception.Message.Should().Be("You don't have permission to edit this project");
    }

    [Fact]
    public async Task DeleteProject_WithValidProject_ShouldDeleteSuccessfully()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Project to Delete",
            Description = "Will be deleted",
            OwnerId = 1,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        // Act
        var result = await _projectService.DeleteProject(_testUser, existingProject.Id);

        // Assert
        result.Should().BeTrue();

        var deletedProject = await _context.Projects.FindAsync(existingProject.Id);
        deletedProject.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProject_WithNonExistentProject_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<HotChocolate.GraphQLException>(
            () => _projectService.DeleteProject(_testUser, 999));

        exception.Message.Should().Be("Project not found");
    }

    [Fact]
    public async Task DeleteProject_WithUnauthorizedUser_ShouldThrowException()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Project to Delete",
            Description = "Will not be deleted",
            OwnerId = 2, // Different owner
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HotChocolate.GraphQLException>(
            () => _projectService.DeleteProject(_testUser, existingProject.Id));

        exception.Message.Should().Be("You don't have permission to delete this project");
    }

    [Fact]
    public async Task RemoveMember_WithValidMember_ShouldRemoveSuccessfully()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test",
            OwnerId = 1,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        var member = new User
        {
            Id = 2,
            Name = "Member",
            Surname = "User",
            Nickname = "member",
            Email = "member@example.com",
            Password = "hashedpassword"
        };

        _context.Projects.Add(project);
        _context.Users.Add(member);
        await _context.SaveChangesAsync();

        // First add member manually (since there's no AddMember method)
        _context.UserProjects.Add(new UserProject { ProjectId = project.Id, UserId = member.Id });
        await _context.SaveChangesAsync();

        // Act - Remove member
        var result = await _projectService.RemoveMember(_testUser, project.Id, member.Id);

        // Assert
        result.Should().BeTrue();

        var userProject = await _context.UserProjects
            .FirstOrDefaultAsync(up => up.ProjectId == project.Id && up.UserId == member.Id);
        userProject.Should().BeNull();
    }

    [Fact]
    public async Task LikeProject_WithValidProject_ShouldLikeSuccessfully()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test",
            OwnerId = 1,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _projectService.LikeProject(_testUser, project.Id);

        // Assert
        result.Should().BeTrue();

        var like = await _context.ProjectLikes
            .FirstOrDefaultAsync(pl => pl.ProjectId == project.Id && pl.UserId == 1);
        like.Should().NotBeNull();
    }

    [Fact]
    public async Task LikeProject_WithAlreadyLikedProject_ShouldReturnFalse()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test",
            OwnerId = 1,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        _context.ProjectLikes.Add(new ProjectLike
        {
            ProjectId = project.Id,
            UserId = 1,
            Created = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _projectService.LikeProject(_testUser, project.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UnlikeProject_WithLikedProject_ShouldUnlikeSuccessfully()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test",
            OwnerId = 1,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        _context.ProjectLikes.Add(new ProjectLike
        {
            ProjectId = project.Id,
            UserId = 1,
            Created = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _projectService.UnlikeProject(_testUser, project.Id);

        // Assert
        result.Should().BeTrue();

        var like = await _context.ProjectLikes
            .FirstOrDefaultAsync(pl => pl.ProjectId == project.Id && pl.UserId == 1);
        like.Should().BeNull();
    }

    [Fact]
    public async Task UnlikeProject_WithNotLikedProject_ShouldReturnFalse()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test",
            OwnerId = 1,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _projectService.UnlikeProject(_testUser, project.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RecordView_WithNewView_ShouldRecordSuccessfully()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test",
            OwnerId = 1,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _projectService.RecordView(_testUser, project.Id);

        // Assert
        result.Should().BeTrue();

        var view = await _context.ProjectViews
            .FirstOrDefaultAsync(pv => pv.ProjectId == project.Id && pv.UserId == 1);
        view.Should().NotBeNull();
        view!.ViewDate.Should().Be(DateTime.UtcNow.Date);
    }

    [Fact]
    public async Task RecordView_WithExistingViewToday_ShouldReturnFalse()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test",
            OwnerId = 1,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        _context.ProjectViews.Add(new ProjectView
        {
            ProjectId = project.Id,
            UserId = 1,
            ViewDate = DateTime.UtcNow.Date,
            Created = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _projectService.RecordView(_testUser, project.Id);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}