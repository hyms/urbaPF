using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.Services;
using UrbaPF.Infrastructure.Data;
using Npgsql;

namespace UrbaPF.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly Func<DateTime> _utcNow;
    private readonly DateTime _fixedDateTime;

    private static readonly string TestPasswordHash;

    static AuthServiceTests()
    {
        var hasher = new PasswordHasher();
        TestPasswordHash = hasher.Hash("password123");
    }

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _connectionFactoryMock = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        _passwordHasher = new PasswordHasher();
        _fixedDateTime = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        _utcNow = () => _fixedDateTime;

        var inMemorySettings = new Dictionary<string, string?> {
            {"JWT_SECRET", "UrbaPFSuperSecretKey2026!ThisMustBeLongEnoughForHS256"},
            {"JWT_EXPIRY_MINUTES", "60"},
            {"REFRESH_TOKEN_EXPIRY_DAYS", "30"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    private AuthService CreateAuthService()
    {
        return new AuthService(_userRepositoryMock.Object, _configuration, _connectionFactoryMock.Object, _passwordHasher, _utcNow);
    }

    private UserDto CreateTestUser(string email = "test@test.com", string password = "password123", int status = 1)
    {
        return new UserDto
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(password),
            FullName = "Test User",
            Role = 1,
            CredibilityLevel = 3,
            Status = status,
            Phone = "12345678",
            CreatedAt = _fixedDateTime,
            LastLoginAt = null
        };
    }

    [Test, Ignore("Requires mock of DbConnection for full unit testing")]
    public async Task Login_WithValidCredentials_ReturnsTokenWithRefreshToken()
    {
        var email = "test@test.com";
        var password = "password123";
        var user = CreateTestUser(email, password);
        
        _userRepositoryMock.Setup(x => x.GetByEmailWithPasswordAsync(email)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.UpdateLastLoginAsync(user.Id)).Returns(Task.CompletedTask);
        _connectionFactoryMock.Setup(x => x.CreateConnection()).Returns(new Npgsql.NpgsqlConnection("Host=localhost;Database=test"));

        var authService = CreateAuthService();
        var result = await authService.LoginAsync(email, password);

        result.Response.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Response!.Token.Should().NotBeNullOrEmpty();
        result.Response.RefreshToken.Should().NotBeNullOrEmpty();
        result.Response.User.Should().NotBeNull();
    }

    [Test]
    public async Task Login_WithInvalidPassword_ReturnsError()
    {
        var email = "test@test.com";
        var user = CreateTestUser(email, "correctpassword");
        var dbConnection = new Mock<System.Data.IDbConnection>();
        
        _connectionFactoryMock.Setup(x => x.CreateConnection()).Returns(dbConnection.Object);
        _userRepositoryMock.Setup(x => x.GetByEmailWithPasswordAsync(email)).ReturnsAsync(user);

        var authService = CreateAuthService();
        var result = await authService.LoginAsync(email, "wrongpassword");

        result.Response.Should().BeNull();
        result.Error.Should().Be("Contraseña incorrecta");
    }

    [Test]
    public async Task Login_WithNonExistentUser_ReturnsError()
    {
        var dbConnection = new Mock<System.Data.IDbConnection>();
        _connectionFactoryMock.Setup(x => x.CreateConnection()).Returns(dbConnection.Object);
        _userRepositoryMock.Setup(x => x.GetByEmailWithPasswordAsync("notfound@test.com")).ReturnsAsync((UserDto?)null);

        var authService = CreateAuthService();
        var result = await authService.LoginAsync("notfound@test.com", "password");

        result.Response.Should().BeNull();
        result.Error.Should().Be("Usuario no encontrado");
    }

    [Test]
    public async Task Login_WithInactiveUser_ReturnsError()
    {
        var email = "inactive@test.com";
        var user = CreateTestUser(email, "password", status: 0);
        var dbConnection = new Mock<System.Data.IDbConnection>();
        
        _connectionFactoryMock.Setup(x => x.CreateConnection()).Returns(dbConnection.Object);
        _userRepositoryMock.Setup(x => x.GetByEmailWithPasswordAsync(email)).ReturnsAsync(user);

        var authService = CreateAuthService();
        var result = await authService.LoginAsync(email, "password");

        result.Response.Should().BeNull();
        result.Error.Should().Be("Usuario inactivo");
    }

    [Test]
    public async Task Register_WithNewEmail_ReturnsUserId()
    {
        var request = new { Email = "new@test.com", Password = "password123", FullName = "New User" };
        var dbConnection = new Mock<System.Data.IDbConnection>();
        
        _connectionFactoryMock.Setup(x => x.CreateConnection()).Returns(dbConnection.Object);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((UserDto?)null);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<CreateUserDto>(), It.IsAny<string>())).ReturnsAsync(Guid.NewGuid());

        var authService = CreateAuthService();
        var result = await authService.RegisterAsync(request.Email, request.Password, request.FullName, null);

        result.UserId.Should().NotBeNull();
        result.Error.Should().BeNull();
    }

    [Test]
    public async Task Register_WithExistingEmail_ReturnsError()
    {
        var existingUser = new UserDto { Email = "existing@test.com" };
        var dbConnection = new Mock<System.Data.IDbConnection>();
        
        _connectionFactoryMock.Setup(x => x.CreateConnection()).Returns(dbConnection.Object);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(existingUser.Email)).ReturnsAsync(existingUser);

        var authService = CreateAuthService();
        var result = await authService.RegisterAsync("existing@test.com", "password", "User", null);

        result.UserId.Should().BeNull();
        result.Error.Should().Be("El email ya está registrado");
    }

    [Test]
    public async Task ChangePassword_WithCorrectOldPassword_ReturnsSuccess()
    {
        var password = "password123";
        var user = CreateTestUser("test@test.com", password);
        var newPassword = "newpassword";
        var dbConnection = new Mock<System.Data.IDbConnection>();
        
        _connectionFactoryMock.Setup(x => x.CreateConnection()).Returns(dbConnection.Object);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.GetByEmailWithPasswordAsync(user.Email)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.UpdatePasswordHashAsync(user.Id, It.IsAny<string>())).Returns(Task.CompletedTask);

        var authService = CreateAuthService();
        var result = await authService.ChangePasswordAsync(user.Id, password, newPassword);

        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Test]
    public async Task ChangePassword_WithIncorrectOldPassword_ReturnsError()
    {
        var user = CreateTestUser();
        var dbConnection = new Mock<System.Data.IDbConnection>();
        
        _connectionFactoryMock.Setup(x => x.CreateConnection()).Returns(dbConnection.Object);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.GetByEmailWithPasswordAsync(user.Email)).ReturnsAsync(user);

        var authService = CreateAuthService();
        var result = await authService.ChangePasswordAsync(user.Id, "wrongoldpassword", "newpassword");

        result.Success.Should().BeFalse();
        result.Error.Should().Be("Contraseña antigua incorrecta.");
    }

    [Test]
    public async Task ChangePassword_WithNonExistentUser_ReturnsError()
    {
        var dbConnection = new Mock<System.Data.IDbConnection>();
        
        _connectionFactoryMock.Setup(x => x.CreateConnection()).Returns(dbConnection.Object);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserDto?)null);

        var authService = CreateAuthService();
        var result = await authService.ChangePasswordAsync(Guid.NewGuid(), "oldpassword", "newpassword");

        result.Success.Should().BeFalse();
        result.Error.Should().Be("Usuario no encontrado.");
    }
}
