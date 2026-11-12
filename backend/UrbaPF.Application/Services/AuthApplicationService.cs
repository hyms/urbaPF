using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UrbaPF.Application.Abstractions;
using UrbaPF.Application.DTOs;
using UrbaPF.Application.Interfaces;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Application.Services;

public class AuthApplicationService : IAuthApplicationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly string _jwtSecret;
    private readonly int _jwtExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;
    private readonly Func<DateTime> _utcNow;

    public AuthApplicationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration,
        IPasswordHasher passwordHasher,
        Func<DateTime> utcNow)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtSecret = _configuration["JWT_SECRET"] ?? "UrbaPFSuperSecretKey2026!ThisMustBeLongEnough";
        _jwtExpiryMinutes = int.Parse(_configuration["JWT_EXPIRY_MINUTES"] ?? "60");
        _refreshTokenExpiryDays = int.Parse(_configuration["REFRESH_TOKEN_EXPIRY_DAYS"] ?? "30");
        _utcNow = utcNow;
    }

    public async Task<(AuthResponseDto? Response, string? Error)> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailWithPasswordAsync(request.Email);
        if (user == null)
            return (null, "Usuario no encontrado");

        if (user.PasswordHash == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return (null, "Contraseña incorrecta");

        if (user.Status != (int)UserStatus.Active)
            return (null, "Usuario inactivo");

        await _userRepository.UpdateLastLoginAsync(user.Id);

        var token = GenerateJwtToken(user.Id, user.Email, (int)user.Role, user.FullName);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = _utcNow().AddMinutes(_jwtExpiryMinutes);

        await _refreshTokenRepository.SaveAsync(user.Id, refreshToken, expiresAt);

        var userDto = await _userRepository.GetByIdAsync(user.Id);
        if (userDto == null)
            return (null, "Error al obtener datos del usuario");

        return (new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = MapToDto(userDto)
        }, null);
    }

    public async Task<(Guid? UserId, string? Error)> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            return (null, "El email ya está registrado");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var createUserDto = new CreateUserDto
        {
            Email = request.Email,
            FullName = request.FullName,
            Phone = request.Phone
        };

        var userId = await _userRepository.CreateAsync(createUserDto, passwordHash, UserRole.Neighbor);

        return (userId, null);
    }

    public async Task<(AuthResponseDto? Response, string? Error)> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (storedToken == null)
            return (null, "Refresh token inválido");

        if (storedToken.RevokedAt != null)
            return (null, "Refresh token revocado");

        if (storedToken.ExpiresAt < _utcNow())
            return (null, "Refresh token expirado");

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user == null)
            return (null, "Usuario no encontrado");

        if (user.Status != (int)UserStatus.Active)
            return (null, "Usuario inactivo");

        await RevokeRefreshTokenAsync(request.RefreshToken);

        var newToken = GenerateJwtToken(user.Id, user.Email, (int)user.Role, user.FullName);
        var newRefreshToken = GenerateRefreshToken();
        var expiresAt = _utcNow().AddMinutes(_jwtExpiryMinutes);

        await _refreshTokenRepository.SaveAsync(user.Id, newRefreshToken, expiresAt);

        return (new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            User = MapToDto(user)
        }, null);
    }

    public async Task<(bool Success, string? Error)> RevokeRefreshTokenAsync(string refreshToken)
    {
        await _refreshTokenRepository.RevokeAsync(refreshToken, _utcNow());
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return (false, "Usuario no encontrado.");

        var userWithHash = await _userRepository.GetByEmailWithPasswordAsync(user.Email);
        if (userWithHash == null || userWithHash.PasswordHash == null || !_passwordHasher.Verify(request.OldPassword, userWithHash.PasswordHash))
            return (false, "Contraseña antigua incorrecta.");

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userRepository.UpdatePasswordHashAsync(userId, newPasswordHash);
        return (true, null);
    }

    private string GenerateJwtToken(Guid userId, string email, int role, string fullName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim("fullName", fullName)
        };

        var token = new JwtSecurityToken(
            issuer: "UrbaPF",
            audience: "UrbaPF",
            claims: claims,
            expires: _utcNow().AddMinutes(_jwtExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static UserDto MapToDto(UrbaPF.Application.DTOs.UserDto user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role,
            CredibilityLevel = user.CredibilityLevel,
            Status = user.Status,
            CondominiumId = user.CondominiumId,
            LotNumber = user.LotNumber,
            StreetAddress = user.StreetAddress,
            PhotoUrl = user.PhotoUrl,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsValidated = user.IsValidated,
            ManagerVotes = user.ManagerVotes,
            FcmToken = user.FcmToken
        };
    }
}