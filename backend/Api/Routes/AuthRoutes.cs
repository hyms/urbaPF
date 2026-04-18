using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Api.DTOs;
using System.Security.Claims;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Api.Routes;

public static class AuthRoutes
{
    public static void MapAuthRoutes(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (IAuthService authService, LoginRequest request) =>
        {
            var (response, error) = await authService.LoginAsync(request.Email, request.Password);
            return response is not null 
                ? Results.Ok(response)
                : Results.Unauthorized();
        });

        app.MapPost("/api/auth/register", async (IAuthService authService, ICondominiumRepository condoRepo, RegisterRequest request) =>
        {
            var condominiums = await condoRepo.GetAllAsync();
            Guid? defaultCondoId = condominiums.FirstOrDefault()?.Id;

            var createDto = new CreateUserDto
            {
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone,
                CondominiumId = defaultCondoId
            };
            var (userId, error) = await authService.RegisterUserAsync(createDto, request.Password, UserRole.Neighbor); // Default to Neighbor role
            return userId.HasValue
                ? Results.Created($"/api/users/{userId}", new { id = userId, message = "Usuario registrado" })
                : Results.BadRequest(new { error });
        });

        app.MapPost("/api/auth/refresh", async (IAuthService authService, RefreshTokenRequest request) =>
        {
            var (response, error) = await authService.RefreshTokenAsync(request.RefreshToken);
            return response is not null
                ? Results.Ok(response)
                : Results.Unauthorized();
        });

        app.MapPost("/api/auth/revoke", async (IAuthService authService, RefreshTokenRequest request) =>
        {
            var (success, error) = await authService.RevokeRefreshTokenAsync(request.RefreshToken);
            return success
                ? Results.Ok(new { message = "Token revoked" })
                : Results.BadRequest(new { error });
        });

        app.MapPost("/api/auth/change-password", async (IAuthService authService, ClaimsPrincipal user, ChangePasswordRequest request) =>
        {
            var userIdClaim = user.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var (success, error) = await authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
            return success
                ? Results.Ok(new { message = "Contraseña actualizada" })
                : Results.BadRequest(new { error });
        }).RequireAuthorization();
    }
}
