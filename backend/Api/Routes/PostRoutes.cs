using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Api.DTOs;
using System.Security.Claims;

namespace UrbaPF.Api.Routes;

public static class PostRoutes
{
    public static void MapPostRoutes(this WebApplication app)
    {
        app.MapGet("/api/posts/{id:guid}", async (Guid id, IPostRepository repo) =>
        {
            var post = await repo.GetByIdAsync(id);
            return post is { } ? Results.Ok(post) : Results.NotFound();
        });

        app.MapPost("/api/condominiums/{id:guid}/posts", async (Guid id, IPostRepository repo, CreatePostRequest request, HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString();
            var postId = await repo.CreateAsync(new CreatePostDto
            {
                Title = request.Title,
                Content = request.Content,
                Category = request.Category,
                IsPinned = request.IsPinned,
                IsAnnouncement = request.IsAnnouncement
            }, Guid.Parse(userId), id);
            return Results.Created($"/api/posts/{postId}", new { id = postId });
        });

        app.MapPut("/api/posts/{id:guid}", async (Guid id, IPostRepository repo, UpdatePostRequest request) =>
        {
            await repo.UpdateAsync(id, new UpdatePostDto
            {
                Title = request.Title,
                Content = request.Content,
                Category = request.Category,
                IsPinned = request.IsPinned,
                IsAnnouncement = request.IsAnnouncement,
                Status = request.Status
            });
            return Results.Ok(new { message = "Publicación actualizada" });
        });

        app.MapDelete("/api/posts/{id:guid}", async (Guid id, IPostRepository repo) =>
        {
            await repo.SoftDeleteAsync(id);
            return Results.Ok(new { message = "Publicación eliminada" });
        });

        app.MapGet("/api/posts/{id:guid}/comments", async (Guid id, ICommentRepository repo) => Results.Ok(await repo.GetByPostAsync(id)));

        app.MapPost("/api/posts/{id:guid}/comments", async (Guid id, ICommentRepository repo, CreateCommentRequest request, HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString();
            var commentId = await repo.CreateAsync(new CreateCommentDto
            {
                Content = request.Content,
                ParentCommentId = request.ParentCommentId
            }, Guid.Parse(userId), id);
            return Results.Created($"/api/comments/{commentId}", new { id = commentId });
        });

        app.MapDelete("/api/comments/{id:guid}", async (Guid id, ICommentRepository repo) =>
        {
            await repo.SoftDeleteAsync(id);
            return Results.Ok(new { message = "Comentario eliminado" });
        });
    }
}
