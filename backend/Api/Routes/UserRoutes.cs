using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Api.DTOs;
using System.Security.Claims;

namespace UrbaPF.Api.Routes;

public static class UserRoutes
{
    public static void MapUserRoutes(this WebApplication app)
    {
        app.MapGet("/api/users", async (IUserRepository repo) => Results.Ok(await repo.GetAllAsync()));

        app.MapGet("/api/users/{id:guid}", async (Guid id, IUserRepository repo) => 
            await repo.GetByIdAsync(id) is { } u ? Results.Ok(u) : Results.NotFound());

        app.MapPost("/api/users", async (IUserRepository repo, CreateUserRequest request) =>
        {
            var userId = await repo.CreateAsync(new CreateUserDto
            {
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone
            }, request.Password);
            return Results.Created($"/api/users/{userId}", new { id = userId });
        });

        app.MapPut("/api/users/{id:guid}", async (Guid id, IUserRepository repo, UpdateUserRequest request) =>
        {
            await repo.UpdateAsync(id, new UpdateUserDto
            {
                FullName = request.FullName,
                Phone = request.Phone,
                FcmToken = request.FcmToken,
                StreetAddress = request.StreetAddress,
                PhotoUrl = request.PhotoUrl
            });
            return Results.Ok(new { message = "Usuario actualizado" });
        });

        app.MapDelete("/api/users/{id:guid}", async (Guid id, IUserRepository repo) =>
        {
            await repo.SoftDeleteAsync(id);
            return Results.Ok(new { message = "Usuario eliminado" });
        });

        app.MapPut("/api/users/{id:guid}/password", async (Guid id, ChangePasswordRequest request, ClaimsPrincipal user, IAuthService authService) =>
        {
            var currentUserId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
            var currentUserRole = int.Parse(user.FindFirst(ClaimTypes.Role)?.Value ?? throw new UnauthorizedAccessException());

            if (currentUserId != id && currentUserRole != 4) // Only owner or Admin can change password
            {
                return Results.Forbid();
            }

            var (success, error) = await authService.ChangePasswordAsync(id, request.OldPassword, request.NewPassword);

            if (!success)
            {
                return Results.BadRequest(new { message = error });
            }
            return Results.Ok(new { message = "Contraseña actualizada con éxito" });
        }).RequireAuthorization();

        app.MapPost("/api/users/{id:guid}/photo", async (Guid id, IFormFile file, ClaimsPrincipal user, IFileStorageService fileStorageService, IUserRepository userRepository) =>
        {
            var currentUserId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
            var currentUserRole = int.Parse(user.FindFirst(ClaimTypes.Role)?.Value ?? throw new UnauthorizedAccessException());

            if (currentUserId != id && currentUserRole != 4) // Only owner or Admin can upload photo
            {
                return Results.Forbid();
            }

            // Use the new FileStorageService to save the file
            using var stream = file.OpenReadStream();
            var (photoUrl, error) = await fileStorageService.SaveFileAsync(stream, file.FileName, file.ContentType);

            if (photoUrl == null)
            {
                return Results.BadRequest(new { message = error });
            }

            // Update the user's PhotoUrl in the database
            var updateDto = new UpdateUserDto { PhotoUrl = photoUrl };
            await userRepository.UpdateAsync(id, updateDto);

            return Results.Ok(new { photoUrl = photoUrl, message = "Foto de perfil actualizada con éxito" });
        }).DisableAntiforgery().RequireAuthorization();
    }
}
