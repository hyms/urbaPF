using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Api.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UrbaPF.Infrastructure.Services;
using UrbaPF.Api.Extensions;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Api.Routes;

public static class UserRoutes
{
    public static void MapUserRoutes(this WebApplication app)
    {
        app.MapGet("/api/users", async (IUserRepository repo) => Results.Ok(await repo.GetAllAsync()))
            .RequireAuthorization()
            .WithMetadata(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = $"{(int)UserRole.Administrator},{(int)UserRole.Manager}" });

        app.MapGet("/api/users/{id:guid}", async (Guid id, IUserRepository repo) => 
            await repo.GetByIdAsync(id) is { } u ? Results.Ok(u) : Results.NotFound())
            .RequireAuthorization();

        app.MapPost("/api/users", async (IAuthService authService, CreateUserRequest request) =>
        {
            var createDto = new CreateUserDto
            {
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone
            };
            var (userId, error) = await authService.RegisterUserAsync(createDto, request.Password, UserRole.Neighbor); // Default to Neighbor role

            return userId.HasValue
                ? Results.Created($"/api/users/{userId}", new { id = userId })
                : Results.BadRequest(new { error });
        })
            .RequireAuthorization()
            .WithMetadata(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = ((int)UserRole.Administrator).ToString() });

        app.MapPut("/api/users/{id:guid}", async (Guid id, IUserRepository repo, UpdateUserRequest request, ClaimsPrincipal user) =>
        {
            var currentUserId = user.GetUserId();
            var currentUserRole = user.GetUserRole();

            var updateDto = new UpdateUserDto
            {
                FullName = request.FullName,
                Phone = request.Phone,
                FcmToken = request.FcmToken,
                StreetAddress = request.StreetAddress,
                PhotoUrl = request.PhotoUrl
            };

            if ((int)currentUserRole == (int)UserRole.Administrator)
            {
                updateDto.Role = request.Role;
            }

            await repo.UpdateAsync(id, updateDto);
            return Results.Ok(new { message = "Usuario actualizado" });
        }).RequireAuthorization();

        app.MapDelete("/api/users/{id:guid}", async (Guid id, IUserRepository repo) =>
        {
            await repo.SoftDeleteAsync(id);
            return Results.Ok(new { message = "Usuario eliminado" });
        })
            .RequireAuthorization()
            .WithMetadata(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = ((int)UserRole.Administrator).ToString() });

        app.MapPut("/api/users/{id:guid}/password", async (Guid id, ChangePasswordRequest request, ClaimsPrincipal user, IAuthService authService) =>
        {
            var currentUserId = user.GetUserId();
            var currentUserRole = user.GetUserRole();

            if (currentUserId != id && (int)currentUserRole != (int)UserRole.Administrator) // Only owner or Admin can change password
            {
                return Results.Problem("No tiene permisos para cambiar esta contraseña.", statusCode: StatusCodes.Status403Forbidden);
            }

            var (success, error) = await authService.ChangePasswordAsync(id, request.OldPassword, request.NewPassword);

            if (!success)
            {
                return Results.BadRequest(new { message = error });
            }
            return Results.Ok(new { message = "Contraseña actualizada con éxito" });
        }).RequireAuthorization();

        app.MapPost("/api/users/{id:guid}/photo", async (Guid id, IFormFile file, ClaimsPrincipal user, IUserService userService) =>
        {
            var userId = await userService.UploadUserPhotoAsync(id, file, user);
            if (userId == null)
            {
                return Results.Problem("Failed to upload photo.", statusCode: StatusCodes.Status500InternalServerError);
            }
            return Results.Ok(new { Message = "Photo uploaded successfully.", UserId = userId });
        }).DisableAntiforgery().RequireAuthorization();

        app.MapPut("/api/users/{id:guid}/fcm-token", async (Guid id, UpdateFcmTokenRequest request, IUserRepository repo, ClaimsPrincipal user) =>
        {
            var currentUserId = user.GetUserId();
            if (currentUserId != id)
            {
                return Results.Forbid();
            }
            await repo.UpdateFcmTokenAsync(id, request.FcmToken);
            return Results.Ok(new { message = "Token actualizado" });
        }).RequireAuthorization();
    }

    public record UpdateFcmTokenRequest(string? FcmToken);
}
