using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Api.DTOs;
using System.Security.Claims;

namespace UrbaPF.Api.Routes;

public static class AlertRoutes
{
    public static void MapAlertRoutes(this WebApplication app)
    {
        app.MapGet("/api/condominiums/{id:guid}/alerts", async (Guid id, IAlertRepository repo) => 
            Results.Ok(await repo.GetActiveByCondominiumAsync(id)));

        app.MapGet("/api/alerts/{id:guid}", async (Guid id, IAlertRepository repo) => 
            await repo.GetByIdAsync(id) is { } a ? Results.Ok(a) : Results.NotFound());

        app.MapPost("/api/condominiums/{id:guid}/alerts", async (Guid id, IAlertRepository repo, IOneSignalService oneSignal, CreateAlertRequest request, HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString();
            var alertId = await repo.CreateAsync(new CreateAlertDto
            {
                AlertType = request.AlertType,
                Message = request.Message,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                DestinationAddress = request.DestinationAddress,
                EstimatedArrival = request.EstimatedArrival
            }, Guid.Parse(userId), id);
            
            await oneSignal.SendToSegmentAsync("Alerta de Seguridad", request.Message, "all");
            
            return Results.Created($"/api/alerts/{alertId}", new { id = alertId });
        });

        app.MapPut("/api/alerts/{id:guid}/status", async (Guid id, IAlertRepository repo, UpdateAlertStatusRequest request) =>
        {
            await repo.UpdateStatusAsync(id, request.Status);
            return Results.Ok(new { message = "Estado de alerta actualizado" });
        });

        app.MapDelete("/api/alerts/{id:guid}", async (Guid id, IAlertRepository repo) =>
        {
            await repo.SoftDeleteAsync(id);
            return Results.Ok(new { message = "Alerta eliminada" });
        });
    }
}
