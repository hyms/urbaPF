using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration; // Added IConfiguration field
    private readonly string _jwtSecret;
    private readonly int _jwtExpiryDays;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration)); // Initialize IConfiguration
        _jwtSecret = _configuration["JWT_SECRET"] ?? "UrbaPFSuperSecretKey2026!ThisMustBeLongEnough";
        _jwtExpiryDays = int.Parse(_configuration["JWT_EXPIRY_DAYS"] ?? "7");
    }

    private string HashPassword(string password)
    {
        // Consider adding salt rounds as a configurable value
        return BCrypt.Net.BCrypt.HashPassword(password, 11);
    }

    private bool VerifyPassword(string providedPassword, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(providedPassword, storedHash);
    }

    public async Task<(string? Token, string? Error)> LoginAsync(string email, string password)
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
        return (token, null);
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
        var userId = await _userRepository.CreateAsync(createDto, passwordHash);

        return (userId, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return (false, "Usuario no encontrado.");

        var userWithHash = await _userRepository.GetByEmailWithPasswordAsync(user.Email);
        if (userWithHash == null || !VerifyPassword(oldPassword, userWithHash.PasswordHash))
            return (false, "Contraseña antigua incorrecta.");

        var newPasswordHash = HashPassword(newPassword);
        await _userRepository.UpdatePasswordHashAsync(userId, newPasswordHash);
        return (true, null);
    }

    private string GenerateJwtToken(Guid userId, string email, int role, string fullName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("id", userId.ToString()),
            new Claim("email", email),
            new Claim("role", role.ToString()),
            new Claim("fullName", fullName)
        };

        var token = new JwtSecurityToken(
            issuer: "UrbaPF",
            audience: "UrbaPF",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jwtExpiryDays),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}