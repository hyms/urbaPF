using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Api.DTOs;
using System.Security.Claims;

namespace UrbaPF.Api.Routes;

public static class IncidentRoutes
{
    public static void MapIncidentRoutes(this WebApplication app)
    {
        app.MapGet("/api/condominiums/{id:guid}/incidents", async (Guid id, IIncidentRepository repo, int? status) => 
            Results.Ok(await repo.GetByCondominiumAsync(id, status)));

        app.MapGet("/api/incidents/{id:guid}", async (Guid id, IIncidentRepository repo) => 
            await repo.GetByIdAsync(id) is { } i ? Results.Ok(i) : Results.NotFound());

        app.MapPost("/api/condominiums/{id:guid}/incidents", async (Guid id, IIncidentRepository repo, IOneSignalService oneSignal, CreateIncidentRequest request, HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString();
            var incidentId = await repo.CreateAsync(new CreateIncidentDto
            {
                Title = request.Title,
                Description = request.Description,
                Type = request.Type,
                Priority = request.Priority,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                LocationDescription = request.LocationDescription,
                OccurredAt = request.OccurredAt
            }, Guid.Parse(userId), id);
            
            await oneSignal.SendToSegmentAsync("Nuevo Incidente", request.Title, "all");
            
            return Results.Created($"/api/incidents/{incidentId}", new { id = incidentId });
        });

        app.MapPut("/api/incidents/{id:guid}/status", async (Guid id, IIncidentRepository repo, UpdateIncidentStatusRequest request) =>
        {
            await repo.UpdateStatusAsync(id, request.Status, request.ResolutionNotes);
            return Results.Ok(new { message = "Estado del incidente actualizado" });
        });

        app.MapDelete("/api/incidents/{id:guid}", async (Guid id, IIncidentRepository repo) =>
        {
            await repo.SoftDeleteAsync(id);
            return Results.Ok(new { message = "Incidente eliminado" });
        });
    }
}
