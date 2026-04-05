using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Api.DTOs;
using System.Security.Claims;

namespace UrbaPF.Api.Routes;

public static class CondominiumRoutes
{
    public static void MapCondominiumRoutes(this WebApplication app)
    {
        app.MapGet("/api/condominiums", async (ICondominiumRepository repo) => Results.Ok(await repo.GetAllAsync()));

        app.MapGet("/api/condominiums/{id:guid}", async (Guid id, ICondominiumRepository repo) => 
            await repo.GetByIdAsync(id) is { } c ? Results.Ok(c) : Results.NotFound());

        app.MapPost("/api/condominiums", async (ICondominiumRepository repo, CreateCondominiumRequest request) =>
        {
            var id = await repo.CreateAsync(new CreateCondominiumDto
            {
                Name = request.Name,
                Address = request.Address,
                Description = request.Description,
                Rules = request.Rules,
                MonthlyFee = request.MonthlyFee,
                Currency = request.Currency
            });
            return Results.Created($"/api/condominiums/{id}", new { id });
        });

        app.MapPut("/api/condominiums/{id:guid}", async (Guid id, ICondominiumRepository repo, UpdateCondominiumRequest request) =>
        {
            await repo.UpdateAsync(id, new UpdateCondominiumDto
            {
                Name = request.Name,
                Address = request.Address,
                Description = request.Description,
                Rules = request.Rules,
                MonthlyFee = request.MonthlyFee,
                IsActive = request.IsActive
            });
            return Results.Ok(new { message = "Condominio actualizado" });
        });

        app.MapDelete("/api/condominiums/{id:guid}", async (Guid id, ICondominiumRepository repo) =>
        {
            await repo.SoftDeleteAsync(id);
            return Results.Ok(new { message = "Condominio eliminado" });
        });
    }
}
