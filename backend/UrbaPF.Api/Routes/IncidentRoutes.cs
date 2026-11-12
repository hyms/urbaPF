using UrbaPF.Domain.Entities;
using UrbaPF.Infrastructure.Services;
using System.Security.Claims;
using UrbaPF.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Api.Routes;

public static class IncidentRoutes
{
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

        app.MapPost("/api/condominiums/{id:guid}/incidents", async (
            Guid id, 
            [FromForm] CreateIncidentRequest request, 
            [FromForm] List<IFormFile>? mediaFiles,
            IIncidentService service, 
            ClaimsPrincipal user
        ) =>
        {
            var userId = user.GetUserId();
            var incidentId = await service.CreateAsync(id, userId, request, mediaFiles);
            return Results.Created($"/api/incidents/{incidentId}", new { id = incidentId });
        }).RequireAuthorization().DisableAntiforgery();

        app.MapPut("/api/incidents/{id:guid}", async (
            Guid id, 
            [FromForm] UpdateIncidentRequest request, 
            [FromForm] List<IFormFile>? mediaFiles,
            IIncidentService service, 
            ClaimsPrincipal user
        ) =>
        {
            var userId = user.GetUserId();
            UserRole userRole = user.GetUserRole();
            var result = await service.UpdateAsync(id, userId, userRole, request, mediaFiles);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization().DisableAntiforgery();

        app.MapPut("/api/incidents/{id:guid}/status", async (Guid id, IIncidentService service, UpdateStatusRequest request, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            UserRole userRole = user.GetUserRole();
            var incident = await service.GetByIdAsync(id);
            
            if (incident is null) return Results.NotFound();
            
            var isReporter = incident.ReporterId == userId;
            var result = await service.UpdateStatusAsync(id, userRole, isReporter, request.Status, request.ResolutionNotes);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();

        app.MapDelete("/api/incidents/{id:guid}", async (Guid id, IIncidentService service, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            UserRole userRole = user.GetUserRole();
            var result = await service.DeleteAsync(id, userId, userRole);
            return result ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();
    }
}

public record UpdateStatusRequest(int Status, string? ResolutionNotes = null);
