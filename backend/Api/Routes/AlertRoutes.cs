using UrbaPF.Infrastructure.Services;
using System.Security.Claims;
using UrbaPF.Domain.Enums;
using UrbaPF.Api.Extensions;

namespace UrbaPF.Api.Routes;

public static class AlertRoutes
{
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
            var userId = user.GetUserId();
            var userRole = user.GetUserRole();
            
            var userRepo = app.Services.GetRequiredService<UrbaPF.Infrastructure.Interfaces.IUserRepository>();
            var dbUser = await userRepo.GetByIdAsync(userId);
            
            if (dbUser == null) return Results.BadRequest("Usuario no encontrado");
            
            var alertId = await service.CreateAsync(id, userId, dbUser.CredibilityLevel, request);
            return Results.Created($"/api/alerts/{alertId}", new { id = alertId });
        }).RequireAuthorization();

        app.MapPost("/api/alerts/{id:guid}/approve", async (Guid id, IAlertService service, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var userRole = user.GetUserRole();
            
            if (userRole < UserRole.Manager) return Results.Forbid();
            
            var result = await service.ApproveAsync(id, userId);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();

        app.MapPost("/api/alerts/{id:guid}/acknowledge", async (Guid id, IAlertService service, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var result = await service.AcknowledgeAsync(id, userId);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();

        app.MapPost("/api/alerts/{id:guid}/resolve", async (Guid id, IAlertService service, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var result = await service.ResolveAsync(id, userId);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();

        app.MapPost("/api/alerts/{id:guid}/resend", async (Guid id, IAlertService service, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var userRole = user.GetUserRole();
            var result = await service.ResendNotificationAsync(id, userId, userRole);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();

        app.MapDelete("/api/alerts/{id:guid}", async (Guid id, IAlertService service, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var userRole = user.GetUserRole();
            var result = await service.DeleteAsync(id, userId, userRole);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();
    }
}
