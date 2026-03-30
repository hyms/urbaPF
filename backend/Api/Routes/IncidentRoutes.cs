using UrbaPF.Domain.Entities;
using UrbaPF.Infrastructure.Services;
using System.Security.Claims;

namespace UrbaPF.Api.Routes;

public static class IncidentRoutes
{
    private const int RoleAdministrator = 4;
    private const int RoleManager = 3;
    private const int RoleGuard = 3;

    public static void MapIncidentRoutes(this WebApplication app)
    {
        app.MapGet("/api/condominiums/{id:guid}/incidents", async (Guid id, int? status, IIncidentService service, ClaimsPrincipal user) =>
        {
            var incidents = await service.GetByCondominiumAsync(id, status);
            return Results.Ok(incidents);
        }).RequireAuthorization();

        app.MapGet("/api/incidents/{id:guid}", async (Guid id, IIncidentService service) =>
        {
            var incident = await service.GetByIdAsync(id);
            if (incident is null) return Results.NotFound();
            return Results.Ok(incident);
        }).RequireAuthorization();

        app.MapPost("/api/condominiums/{id:guid}/incidents", async (Guid id, IIncidentService service, CreateIncidentRequest request, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var incidentId = await service.CreateAsync(id, userId, request);
            return Results.Created($"/api/incidents/{incidentId}", new { id = incidentId });
        }).RequireAuthorization();

        app.MapPut("/api/incidents/{id:guid}", async (Guid id, IIncidentService service, UpdateIncidentRequest request, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var userRole = GetUserRole(user);
            var result = await service.UpdateAsync(id, userId, userRole, request);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();

        app.MapPut("/api/incidents/{id:guid}/status", async (Guid id, IIncidentService service, UpdateStatusRequest request, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var userRole = GetUserRole(user);
            var incident = await service.GetByIdAsync(id);
            
            if (incident is null) return Results.NotFound();
            
            var isReporter = incident.ReporterId == userId;
            var result = await service.UpdateStatusAsync(id, userRole, isReporter, request.Status, request.ResolutionNotes);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();

        app.MapDelete("/api/incidents/{id:guid}", async (Guid id, IIncidentService service, ClaimsPrincipal user) =>
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

public record UpdateStatusRequest(int Status, string? ResolutionNotes = null);
