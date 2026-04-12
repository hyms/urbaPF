using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Api.DTOs;
using UrbaPF.Api.Extensions;
using System.Security.Claims;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Api.Routes;

public static class PostRoutes
{
    public static void MapPostRoutes(this WebApplication app)
    {
        app.MapGet("/api/condominiums/{id:guid}/posts", async (Guid id, IPostService service, ClaimsPrincipal user) =>
        {
            UserRole userRole = user.GetUserRole();
            var posts = await service.GetByCondominiumAsync(id);
            
            if (userRole == UserRole.Administrator || userRole == UserRole.Manager)
            {
                return Results.Ok(posts);
            }
            
            var approvedPosts = posts.Where(p => p.Status == (int)PostStatus.Published);
            return Results.Ok(approvedPosts);
        }).RequireAuthorization();

        app.MapGet("/api/posts/{id:guid}", async (Guid id, IPostService service, ClaimsPrincipal user) =>
        {
            var post = await service.GetByIdAsync(id);
            if (post is null) return Results.NotFound();
            
            UserRole userRole = user.GetUserRole();
            var userId = user.GetUserId();
            
            if (post.Status == (int)PostStatus.Published || userRole == UserRole.Administrator || userRole == UserRole.Manager || post.AuthorId == userId)
            {
                return Results.Ok(post);
            }
            
            return Results.NotFound();
        }).RequireAuthorization();

        app.MapPost("/api/condominiums/{id:guid}/posts", async (Guid id, IPostService service, CreatePostRequest request, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var postId = await service.CreateAsync(id, userId, new CreatePostDto
            {
                Title = request.Title,
                Content = request.Content,
                IsPinned = request.IsPinned,
                IsAnnouncement = request.IsAnnouncement,
                Category = request.Category
            });
            
            return Results.Created($"/api/posts/{postId}", new { id = postId });
        }).RequireAuthorization();

        app.MapPut("/api/posts/{id:guid}", async (Guid id, IPostService service, UpdatePostRequest request, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            UserRole userRole = user.GetUserRole();
            
            var success = await service.UpdateAsync(id, userId, (UserRole)userRole, new UpdatePostDto
            {
                Title = request.Title,
                Content = request.Content,
                IsPinned = request.IsPinned,
                IsAnnouncement = request.IsAnnouncement,
                Category = request.Category,
                Status = request.Status
            });
            
            return success ? Results.Ok(new { message = "Publicación actualizada" }) : Results.Forbid();
        }).RequireAuthorization();

        app.MapDelete("/api/posts/{id:guid}", async (Guid id, IPostService service, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            UserRole userRole = user.GetUserRole();
            
            var success = await service.DeleteAsync(id, userId, (UserRole)userRole);
            
            return success ? Results.Ok(new { message = "Publicación eliminada" }) : Results.Forbid();
        }).RequireAuthorization();
        
        // ... Comments routes omitted for brevity in thought process, I will keep them as is.
        app.MapGet("/api/posts/{id:guid}/comments", async (Guid id, ICommentRepository repo) => Results.Ok(await repo.GetByPostAsync(id)))
            .RequireAuthorization();

        app.MapPost("/api/posts/{id:guid}/comments", async (Guid id, ICommentRepository repo, CreateCommentRequest request, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var commentId = await repo.CreateAsync(new CreateCommentDto
            {
                Content = request.Content,
                ParentCommentId = request.ParentCommentId
            }, userId, id);
            return Results.Created($"/api/comments/{commentId}", new { id = commentId });
        }).RequireAuthorization();

        app.MapDelete("/api/comments/{id:guid}", async (Guid id, ICommentRepository repo, ClaimsPrincipal user) =>
        {
            UserRole currentUserRole = user.GetUserRole();
            
            if (currentUserRole != UserRole.Administrator)
            {
                return Results.Forbid();
            }
            
            await repo.SoftDeleteAsync(id);
            return Results.Ok(new { message = "Comentario eliminado" });
        }).RequireAuthorization();
    }
}