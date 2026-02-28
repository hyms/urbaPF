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
    private readonly string _jwtSecret;
    private readonly int _jwtExpiryDays;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _jwtSecret = configuration["JWT_SECRET"] ?? "UrbaPFSuperSecretKey2026!ThisMustBeLongEnough";
        _jwtExpiryDays = int.Parse(configuration["JWT_EXPIRY_DAYS"] ?? "7");
    }

    public async Task<(string? Token, string? Error)> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailWithPasswordAsync(email);
        if (user == null)
            return (null, "Usuario no encontrado");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
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

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, 11);
        var createDto = new CreateUserDto 
        { 
            Email = email, 
            FullName = fullName, 
            Phone = phone 
        };
        var userId = await _userRepository.CreateAsync(createDto, passwordHash);

        return (userId, null);
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
            new Claim(ClaimTypes.Name, fullName)
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
