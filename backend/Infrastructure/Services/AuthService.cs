using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly string _jwtSecret;
    private readonly int _jwtExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;
    private readonly Func<DateTime> _utcNow;

    public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
        : this(userRepository, refreshTokenRepository, configuration, new PasswordHasher(), () => DateTime.UtcNow)
    {
    }

    public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration, IPasswordHasher passwordHasher, Func<DateTime> utcNow)
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

    private string HashPassword(string password)
    {
        return _passwordHasher.Hash(password);
    }

    private bool VerifyPassword(string providedPassword, string storedHash)
    {
        return _passwordHasher.Verify(providedPassword, storedHash);
    }

    public async Task<(AuthResponseDto? Response, string? Error)> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailWithPasswordAsync(email);
        if (user == null)
            return (null, "Usuario no encontrado");

        if (user.PasswordHash == null || !VerifyPassword(password, user.PasswordHash))
            return (null, "Contraseña incorrecta");

        if (user.Status != 1)
            return (null, "Usuario inactivo");

        await _userRepository.UpdateLastLoginAsync(user.Id);

        var token = GenerateJwtToken(user.Id, user.Email, user.Role, user.FullName);
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
            User = userDto
        }, null);
    }

    public async Task<(Guid? UserId, string? Error)> RegisterAsync(string email, string password, string fullName, string? phone)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
            return (null, "El email ya está registrado");

        var passwordHash = HashPassword(password);
        var createDto = new CreateUserDto 
        { 
            Email = email, 
            FullName = fullName, 
            Phone = phone 
        };
        var userId = await _userRepository.CreateAsync(createDto, passwordHash, 2); // Default to Neighbor role

        return (userId, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return (false, "Usuario no encontrado.");

        var userWithHash = await _userRepository.GetByEmailWithPasswordAsync(user.Email);
        if (userWithHash == null || userWithHash.PasswordHash == null || !VerifyPassword(oldPassword, userWithHash.PasswordHash))
            return (false, "Contraseña antigua incorrecta.");

        var newPasswordHash = HashPassword(newPassword);
        await _userRepository.UpdatePasswordHashAsync(userId, newPasswordHash);
        return (true, null);
    }

    public async Task<(AuthResponseDto? Response, string? Error)> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (storedToken == null)
            return (null, "Refresh token inválido");

        if (storedToken.RevokedAt != null)
            return (null, "Refresh token revocado");

        if (storedToken.ExpiresAt < _utcNow())
            return (null, "Refresh token expirado");

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user == null)
            return (null, "Usuario no encontrado");

        if (user.Status != 1)
            return (null, "Usuario inactivo");

        await RevokeRefreshTokenAsync(refreshToken);

        var newToken = GenerateJwtToken(user.Id, user.Email, user.Role, user.FullName);
        var newRefreshToken = GenerateRefreshToken();
        var expiresAt = _utcNow().AddMinutes(_jwtExpiryMinutes);

        await _refreshTokenRepository.SaveAsync(user.Id, newRefreshToken, expiresAt);

        return (new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            User = user
        }, null);
    }

    public async Task<(bool Success, string? Error)> RevokeRefreshTokenAsync(string refreshToken)
    {
        await _refreshTokenRepository.RevokeAsync(refreshToken, _utcNow());
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
}
