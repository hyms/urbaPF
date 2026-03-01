using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Api.DTOs;
using System.Security.Claims;

namespace UrbaPF.Api.Routes;

public static class PollRoutes
{
    public static void MapPollRoutes(this WebApplication app)
    {
        app.MapGet("/api/condominiums/{id:guid}/polls", async (Guid id, IPollRepository repo) => 
            Results.Ok(await repo.GetByCondominiumAsync(id)));

        app.MapGet("/api/polls/{id:guid}", async (Guid id, IPollRepository repo) => 
            await repo.GetByIdAsync(id) is { } p ? Results.Ok(p) : Results.NotFound());

        app.MapPost("/api/condominiums/{id:guid}/polls", async (Guid id, IPollRepository repo, IOneSignalService oneSignal, CreatePollRequest request, HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString();
            var pollId = await repo.CreateAsync(new CreatePollDto
            {
                Title = request.Title,
                Description = request.Description,
                Options = request.Options,
                PollType = request.PollType,
                StartsAt = request.StartsAt,
                EndsAt = request.EndsAt,
                RequiresJustification = request.RequiresJustification,
                MinRoleToVote = request.MinRoleToVote
            }, Guid.Parse(userId), id);
            
            await oneSignal.SendToSegmentAsync("Nueva Votación", request.Title, "all");
            
            return Results.Created($"/api/polls/{pollId}", new { id = pollId });
        });

        app.MapPut("/api/polls/{id:guid}", async (Guid id, IPollRepository repo, UpdatePollRequest request) =>
        {
            await repo.UpdateAsync(id, new UpdatePollDto
            {
                Title = request.Title,
                Description = request.Description,
                Options = request.Options,
                StartsAt = request.StartsAt,
                EndsAt = request.EndsAt,
                Status = request.Status
            });
            return Results.Ok(new { message = "Votación actualizada" });
        });

        app.MapDelete("/api/polls/{id:guid}", async (Guid id, IPollRepository repo) =>
        {
            await repo.SoftDeleteAsync(id);
            return Results.Ok(new { message = "Votación eliminada" });
        });

        app.MapPost("/api/polls/{id:guid}/vote", async (Guid id, IPollRepository pollRepo, IVoteRepository voteRepo, CreateVoteRequest request, HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString();
            var poll = await pollRepo.GetByIdAsync(id);
            var ipAddress = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            if (poll is null)
                return Results.NotFound(new { error = "Votación no encontrada" });
                
            if (poll.Status != 1)
                return Results.BadRequest(new { error = "La votación no está activa" });
                
            if (DateTime.UtcNow < poll.StartsAt || DateTime.UtcNow > poll.EndsAt)
                return Results.BadRequest(new { error = "La votación no está dentro del período establecido" });
            
            var hasVoted = await voteRepo.HasUserVotedAsync(id, Guid.Parse(userId));
            if (hasVoted)
                return Results.BadRequest(new { error = "Ya has votado en esta elección" });
                
            var signature = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes($"{poll.Id}:{userId}:{request.OptionIndex}:{DateTime.UtcNow:O}")));
            
            await voteRepo.CreateAsync(Guid.Parse(userId), id, request.OptionIndex, signature, ipAddress);
            return Results.Ok(new { message = "Voto registrado" });
        });

        app.MapGet("/api/polls/{id:guid}/votes", async (Guid id, IVoteRepository repo) => 
            Results.Ok(new { votes = await repo.GetByPollAsync(id), results = await repo.GetResultsAsync(id) }));
    }
}
