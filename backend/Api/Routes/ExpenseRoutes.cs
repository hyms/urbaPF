using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UrbaPF.Api.Extensions;
using UrbaPF.Domain.Enums;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Api.Routes;

public static class ExpenseRoutes
{
    public static void MapExpenseRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/expenses").RequireAuthorization();

        group.MapGet("/", async (IExpenseService service, ClaimsPrincipal user) =>
        {
            var comunidadId = user.GetCondominiumId();
            if (comunidadId == Guid.Empty) return Results.BadRequest("User does not belong to a condominium");
            
            var result = await service.GetExpensesAsync(comunidadId);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/summary", async (IExpenseService service, ClaimsPrincipal user) =>
        {
            var comunidadId = user.GetCondominiumId();
            if (comunidadId == Guid.Empty) return Results.BadRequest("User does not belong to a condominium");
            
            var result = await service.GetExpenseSummaryAsync(comunidadId);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/{id:guid}", async (Guid id, IExpenseService service) =>
        {
            var result = await service.GetExpenseByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapPost("/", async (CreateExpenseDto dto, IExpenseService service, ClaimsPrincipal user) =>
        {
            var comunidadId = user.GetCondominiumId();
            var usuarioId = user.GetUserId();
            var role = user.GetUserRole();

            if (role < UserRole.Manager) return Results.Forbid();

            var result = await service.CreateExpenseAsync(comunidadId, usuarioId, dto);
            return result.IsSuccess 
                ? Results.Created($"/api/expenses/{result.Value}", new { id = result.Value }) 
                : Results.BadRequest(result.Error);
        });

        group.MapPut("/{id:guid}", async (Guid id, CreateExpenseDto dto, IExpenseService service, ClaimsPrincipal user) =>
        {
            var usuarioId = user.GetUserId();
            var role = user.GetUserRole();

            if (role < UserRole.Manager) return Results.Forbid();

            var result = await service.UpdateExpenseAsync(id, usuarioId, dto);
            return result.IsSuccess ? Results.Ok() : Results.NotFound(result.Error);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IExpenseService service, ClaimsPrincipal user) =>
        {
            var usuarioId = user.GetUserId();
            var role = user.GetUserRole();

            if (role < UserRole.Manager) return Results.Forbid();

            var result = await service.DeleteExpenseAsync(id, usuarioId);
            return result.IsSuccess ? Results.Ok() : Results.NotFound(result.Error);
        });
    }
}