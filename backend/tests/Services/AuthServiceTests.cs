using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.Services;

namespace UrbaPF.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        
        var inMemorySettings = new Dictionary<string, string?> {
            {"JWT_SECRET", "UrbaPFSuperSecretKey2026!ThisMustBeLongEnoughForHS256"},
            {"JWT_EXPIRY_DAYS", "7"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var email = "test@test.com";
        var password = "password123";
        var user = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = "Test User",
            Role = 1,
            CredibilityLevel = 3,
            Status = 1
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithPasswordAsync(email)).ReturnsAsync(user);

        var authService = new AuthService(_userRepositoryMock.Object, _configuration);
        var result = await authService.LoginAsync(email, password);

        result.Token.Should().NotBeNullOrEmpty();
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsError()
    {
        var email = "test@test.com";
        var user = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            FullName = "Test User",
            Status = 1
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithPasswordAsync(email)).ReturnsAsync(user);

        var authService = new AuthService(_userRepositoryMock.Object, _configuration);
        var result = await authService.LoginAsync(email, "wrongpassword");

        result.Token.Should().BeNull();
        result.Error.Should().Be("Contraseña incorrecta");
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsError()
    {
        _userRepositoryMock.Setup(x => x.GetByEmailWithPasswordAsync("notfound@test.com")).ReturnsAsync((UserDto?)null);

        var authService = new AuthService(_userRepositoryMock.Object, _configuration);
        var result = await authService.LoginAsync("notfound@test.com", "password");

        result.Token.Should().BeNull();
        result.Error.Should().Be("Usuario no encontrado");
    }

    [Fact]
    public async Task Login_WithInactiveUser_ReturnsError()
    {
        var email = "inactive@test.com";
        var user = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            FullName = "Test User",
            Status = 0
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithPasswordAsync(email)).ReturnsAsync(user);

        var authService = new AuthService(_userRepositoryMock.Object, _configuration);
        var result = await authService.LoginAsync(email, "password");

        result.Token.Should().BeNull();
        result.Error.Should().Be("Usuario inactivo");
    }

    [Fact]
    public async Task Register_WithNewEmail_ReturnsUserId()
    {
        var request = new { Email = "new@test.com", Password = "password123", FullName = "New User" };
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((UserDto?)null);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<CreateUserDto>(), It.IsAny<string>())).ReturnsAsync(Guid.NewGuid());

        var authService = new AuthService(_userRepositoryMock.Object, _configuration);
        var result = await authService.RegisterAsync(request.Email, request.Password, request.FullName, null);

        result.UserId.Should().NotBeNull();
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsError()
    {
        var existingUser = new UserDto { Email = "existing@test.com" };
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(existingUser.Email)).ReturnsAsync(existingUser);

        var authService = new AuthService(_userRepositoryMock.Object, _configuration);
        var result = await authService.RegisterAsync("existing@test.com", "password", "User", null);

        result.UserId.Should().BeNull();
        result.Error.Should().Be("El email ya está registrado");
    }
}
