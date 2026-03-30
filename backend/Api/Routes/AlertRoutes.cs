using UrbaPF.Infrastructure.Services;
using System.Security.Claims;

namespace UrbaPF.Api.Routes;

public static class AlertRoutes
{
    private const int RoleAdministrator = 4;
    private const int RoleManager = 3;

    public static void MapAlertRoutes(this WebApplication app)
    {
        app.MapGet("/api/condominiums/{id:guid}/alerts", async (Guid id, int? status, IAlertService service, ClaimsPrincipal user) =>
        {
            var alerts = await service.GetByCondominiumAsync(id, status);
            return Results.Ok(alerts);
        }).RequireAuthorization();

        app.MapGet("/api/condominiums/{id:guid}/alerts/active", async (Guid id, IAlertService service) =>
        {
            var alerts = await service.GetActiveAlertsAsync(id);
            return Results.Ok(alerts);
        }).RequireAuthorization();

        app.MapGet("/api/alerts/{id:guid}", async (Guid id, IAlertService service) =>
        {
            var alert = await service.GetByIdAsync(id);
            if (alert is null) return Results.NotFound();
            return Results.Ok(alert);
        }).RequireAuthorization();

        app.MapPost("/api/condominiums/{id:guid}/alerts/panic", async (Guid id, IAlertService service, CreateAlertRequest request, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var userRole = GetUserRole(user);
            
            var userRepo = app.Services.GetRequiredService<UrbaPF.Infrastructure.Interfaces.IUserRepository>();
            var dbUser = await userRepo.GetByIdAsync(userId);
            
            if (dbUser == null) return Results.BadRequest("Usuario no encontrado");
            
            var alertId = await service.CreateAsync(id, userId, dbUser.CredibilityLevel, request);
            return Results.Created($"/api/alerts/{alertId}", new { id = alertId });
        }).RequireAuthorization();

        app.MapPost("/api/alerts/{id:guid}/approve", async (Guid id, IAlertService service, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var userRole = GetUserRole(user);
            
            if (userRole < RoleManager) return Results.Forbid();
            
            var result = await service.ApproveAsync(id, userId);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();

        app.MapPost("/api/alerts/{id:guid}/acknowledge", async (Guid id, IAlertService service, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var result = await service.AcknowledgeAsync(id, userId);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();

        app.MapPost("/api/alerts/{id:guid}/resolve", async (Guid id, IAlertService service, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var result = await service.ResolveAsync(id, userId);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();

        app.MapPost("/api/alerts/{id:guid}/resend", async (Guid id, IAlertService service, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var userRole = GetUserRole(user);
            var result = await service.ResendNotificationAsync(id, userId, userRole);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();

        app.MapDelete("/api/alerts/{id:guid}", async (Guid id, IAlertService service, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var userRole = GetUserRole(user);
            var result = await service.DeleteAsync(id, userId, userRole);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }

    private static int GetUserRole(ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
        return roleClaim != null ? int.Parse(roleClaim) : 0;
    }
}
