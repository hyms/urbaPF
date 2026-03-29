using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.Services;

namespace UrbaPF.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _userService = new UserService(_userRepositoryMock.Object, _fileStorageServiceMock.Object);
    }

    [Test]
    public async Task UploadUserPhotoAsync_WithValidFile_ReturnsUserId()
    {
        var userId = Guid.NewGuid();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("photo.jpg");
        
        var claims = new System.Security.Claims.Claim[]
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString())
        };
        var principal = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(claims));

        _fileStorageServiceMock
            .Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("https://storage.example.com/photo.jpg");
        
        _userRepositoryMock
            .Setup(x => x.UpdateUserPhotoUrlAsync(userId, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _userService.UploadUserPhotoAsync(userId, fileMock.Object, principal);

        result.Should().Be(userId);
    }

    [Test]
    public async Task UploadUserPhotoAsync_WithNullFile_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var claims = new System.Security.Claims.Claim[]
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString())
        };
        var principal = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(claims));

        var result = await _userService.UploadUserPhotoAsync(userId, null!, principal);

        result.Should().BeNull();
    }

    [Test]
    public async Task UploadUserPhotoAsync_WithEmptyFile_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        
        var claims = new System.Security.Claims.Claim[]
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString())
        };
        var principal = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(claims));

        var result = await _userService.UploadUserPhotoAsync(userId, fileMock.Object, principal);

        result.Should().BeNull();
    }

    [Test]
    public async Task UploadUserPhotoAsync_WhenUploadFails_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        
        var claims = new System.Security.Claims.Claim[]
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString())
        };
        var principal = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(claims));

        _fileStorageServiceMock
            .Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        var result = await _userService.UploadUserPhotoAsync(userId, fileMock.Object, principal);

        result.Should().BeNull();
    }
}
