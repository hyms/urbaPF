using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Api.DTOs;
using System.Security.Claims;

namespace UrbaPF.Api.Routes;

public static class AuthRoutes
{
    public static void MapAuthRoutes(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (IAuthService authService, LoginRequest request) =>
        {
            var (token, error) = await authService.LoginAsync(request.Email, request.Password);
            return token is not null 
                ? Results.Ok(new { token, message = "Login exitoso" })
                : Results.Unauthorized();
        });

        app.MapPost("/api/auth/register", async (IAuthService authService, RegisterRequest request) =>
        {
            var (userId, error) = await authService.RegisterAsync(request.Email, request.Password, request.FullName, request.Phone);
            return userId.HasValue
                ? Results.Created($"/api/users/{userId}", new { id = userId, message = "Usuario registrado" })
                : Results.BadRequest(new { error });
        });
    }
}
