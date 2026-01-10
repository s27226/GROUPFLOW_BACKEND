using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Moq;
using NAME_WIP_BACKEND.Controllers;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using Xunit;

namespace NAME_WIP_BACKEND.Tests.UnitTests.Controllers;

/// <summary>
/// Unit tests for authentication mutations (register and login)
/// </summary>
public class AuthMutationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AuthMutation _authMutation;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly DefaultHttpContext _httpContext;

    public AuthMutationTests()
    {
        // Set JWT_SECRET environment variable for tests
        Environment.SetEnvironmentVariable("JWT_SECRET", "test-secret-key-minimum-32-characters-long-for-security");
        
        // Setup in-memory database for testing
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _authMutation = new AuthMutation();

        // Setup HttpContext mock for cookie management
        _httpContext = new DefaultHttpContext();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContext);
    }

    [Fact]
    public async Task RegisterUser_WithValidInput_ShouldCreateUser()
    {
        // Arrange
        var input = new UserRegisterInput(
            Name: "John",
            Surname: "Doe",
            Nickname: "johndoe",
            Email: "john@example.com",
            Password: "Password123!"
        );

        // Act
        var result = await _authMutation.RegisterUser(_context, _httpContextAccessorMock.Object, input);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("John");
        result.Email.Should().Be("john@example.com");
        
        // Tokens are now in HTTP-only cookies, not in the response
        var cookies = _httpContext.Response.Headers["Set-Cookie"].ToString();
        cookies.Should().Contain("access_token");
        cookies.Should().Contain("refresh_token");

        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == input.Email);
        userInDb.Should().NotBeNull();
        userInDb!.Name.Should().Be("John");
        userInDb.Surname.Should().Be("Doe");
        userInDb.Nickname.Should().Be("johndoe");
        
        // Password should be hashed
        userInDb.Password.Should().NotBe(input.Password);
        BCrypt.Net.BCrypt.Verify(input.Password, userInDb.Password).Should().BeTrue();
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var existingUser = new User
        {
            Name = "Existing",
            Surname = "User",
            Nickname = "existing",
            Email = "duplicate@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password")
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var input = new UserRegisterInput(
            Name: "New",
            Surname: "User",
            Nickname: "newuser",
            Email: "duplicate@example.com",
            Password: "Password123!"
        );

        // Act
        Func<Task> act = async () => await _authMutation.RegisterUser(_context, _httpContextAccessorMock.Object, input);

        // Assert
        await act.Should().ThrowAsync<HotChocolate.GraphQLException>()
            .WithMessage("*Email already exists*");
    }

    [Fact]
    public async Task LoginUser_WithValidCredentials_ShouldReturnAuthPayload()
    {
        // Arrange
        var password = "Password123!";
        var user = new User
        {
            Name = "Login",
            Surname = "Test",
            Nickname = "logintest",
            Email = "login@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword(password)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var input = new UserLoginInput(
            Email: "login@example.com",
            Password: password
        );

        // Act
        var result = await _authMutation.LoginUser(_context, _httpContextAccessorMock.Object, input);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("login@example.com");
        result.Name.Should().Be("Login");
        
        // Tokens are now in HTTP-only cookies, not in the response
        var cookies = _httpContext.Response.Headers["Set-Cookie"].ToString();
        cookies.Should().Contain("access_token");
        cookies.Should().Contain("refresh_token");
    }

    [Fact]
    public async Task LoginUser_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        var input = new UserLoginInput(
            Email: "nonexistent@example.com",
            Password: "Password123!"
        );

        // Act
        Func<Task> act = async () => await _authMutation.LoginUser(_context, _httpContextAccessorMock.Object, input);

        // Assert
        await act.Should().ThrowAsync<HotChocolate.GraphQLException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task LoginUser_WithInvalidPassword_ShouldThrowException()
    {
        // Arrange
        var user = new User
        {
            Name = "Test",
            Surname = "User",
            Nickname = "testuser",
            Email = "test@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var input = new UserLoginInput(
            Email: "test@example.com",
            Password: "WrongPassword"
        );

        // Act
        Func<Task> act = async () => await _authMutation.LoginUser(_context, _httpContextAccessorMock.Object, input);

        // Assert
        await act.Should().ThrowAsync<HotChocolate.GraphQLException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task RegisterUser_ShouldHashPassword()
    {
        // Arrange
        var input = new UserRegisterInput(
            Name: "Hash",
            Surname: "Test",
            Nickname: "hashtest",
            Email: "hash@example.com",
            Password: "PlainTextPassword"
        );

        // Act
        var result = await _authMutation.RegisterUser(_context, _httpContextAccessorMock.Object, input);

        // Assert
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == input.Email);
        user.Should().NotBeNull();
        user!.Password.Should().NotBe(input.Password);
        user.Password.Should().StartWith("$2");  // BCrypt hash indicator
        BCrypt.Net.BCrypt.Verify(input.Password, user.Password).Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
