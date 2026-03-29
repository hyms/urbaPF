using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Api.DTOs;
using System.Security.Claims;

namespace UrbaPF.Api.Routes;

public static class PollRoutes
{
    public static void MapPollRoutes(this WebApplication app)
    {
        app.MapGet("/api/condominiums/{id:guid}/polls", async (Guid id, IPollService pollService) => 
            Results.Ok(await pollService.GetByCondominiumAsync(id)));

        app.MapGet("/api/polls/{id:guid}", async (Guid id, IPollService pollService) => 
            await pollService.GetByIdAsync(id) is { } p ? Results.Ok(p) : Results.NotFound());

        app.MapPost("/api/condominiums/{id:guid}/polls", async (Guid id, IPollService pollService, CreatePollRequest request, HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString();
            var userRole = GetUserRole(ctx.User);
            
            var result = await pollService.CreateAsync(new CreatePollDto
            {
                Title = request.Title,
                Description = request.Description,
                Options = request.Options,
                PollType = request.PollType,
                StartsAt = request.StartsAt,
                EndsAt = request.EndsAt,
                RequiresJustification = request.RequiresJustification,
                MinRoleToVote = request.MinRoleToVote,
                Status = 0
            }, Guid.Parse(userId), id, userRole);
            
            if (result is { } pollResult)
            {
                return Results.Created($"/api/polls/{pollResult.pollId}", new { id = pollResult.pollId, status = pollResult.status });
            }
            
            return Results.BadRequest(new { error = "Error al crear la votación" });
        });

        app.MapPut("/api/polls/{id:guid}", async (Guid id, IPollService pollService, UpdatePollRequest request, HttpContext ctx) =>
        {
            var userRole = GetUserRole(ctx.User);
            var result = await pollService.UpdateAsync(id, new UpdatePollDto
            {
                Title = request.Title,
                Description = request.Description,
                Options = request.Options,
                StartsAt = request.StartsAt,
                EndsAt = request.EndsAt,
                Status = request.Status
            }, userRole);

            if (!result.success)
                return Results.BadRequest(new { error = result.error });

            return Results.Ok(new { message = "Votación actualizada" });
        });

        app.MapDelete("/api/polls/{id:guid}", async (Guid id, IPollService pollService, HttpContext ctx) =>
        {
            var userRole = GetUserRole(ctx.User);
            var result = await pollService.DeleteAsync(id, userRole);

            if (!result.success)
                return Results.BadRequest(new { error = result.error });

            return Results.Ok(new { message = "Votación eliminada" });
        });

        app.MapPost("/api/polls/{id:guid}/vote", async (Guid id, IPollService pollService, CreateVoteRequest request, HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString();
            var userRole = GetUserRole(ctx.User);
            var ipAddress = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            var result = await pollService.VoteAsync(id, Guid.Parse(userId), userRole, request.OptionIndex, ipAddress);
            
            if (!result.success)
                return Results.BadRequest(new { error = result.error });

            return Results.Ok(new { message = "Voto registrado" });
        });

        app.MapGet("/api/polls/{id:guid}/votes", async (Guid id, IPollService pollService) => 
            Results.Ok(await pollService.GetResultsAsync(id)));

        app.MapGet("/api/polls/{id:guid}/verify", async (Guid id, Guid userId, int optionIndex, string signature, IPollService pollService) =>
        {
            var result = await pollService.VerifyVoteAsync(id, userId, optionIndex, signature);
            return result.isValid ? Results.Ok(new { valid = true }) : Results.BadRequest(new { valid = false, error = result.error });
        });
    }
    
    private static int GetUserRole(ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
        return roleClaim != null ? int.Parse(roleClaim) : 0;
    }
}
