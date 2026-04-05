using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Api.DTOs;
using System.Security.Claims;

namespace UrbaPF.Api.Routes;

public static class PostRoutes
{
    private const int RoleAdministrator = 4;
    private const int RoleManager = 3;
    private const int RoleNeighbor = 2;
    
    private const int StatusPending = 0;
    private const int StatusApproved = 1;
    private const int StatusRejected = 2;

    public static void MapPostRoutes(this WebApplication app)
    {
        app.MapGet("/api/condominiums/{id:guid}/posts", async (Guid id, IPostRepository repo, ClaimsPrincipal user) =>
        {
            var userRole = GetUserRole(user);
            var posts = await repo.GetByCondominiumAsync(id);
            
            if (userRole == RoleAdministrator || userRole == RoleManager)
            {
                return Results.Ok(posts);
            }
            
            var approvedPosts = posts.Where(p => p.Status == StatusApproved);
            return Results.Ok(approvedPosts);
        }).RequireAuthorization();

        app.MapGet("/api/posts/{id:guid}", async (Guid id, IPostRepository repo, ClaimsPrincipal user) =>
        {
            var post = await repo.GetByIdAsync(id);
            if (post is null) return Results.NotFound();
            
            var userRole = GetUserRole(user);
            var userId = GetUserId(user);
            
            if (post.Status == StatusApproved || userRole == RoleAdministrator || userRole == RoleManager || post.AuthorId == userId)
            {
                return Results.Ok(post);
            }
            
            return Results.NotFound();
        }).RequireAuthorization();

        app.MapPost("/api/condominiums/{id:guid}/posts", async (Guid id, IPostRepository repo, CreatePostRequest request, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var userRole = GetUserRole(user);
            
            var status = (userRole == RoleAdministrator || userRole == RoleManager) ? StatusApproved : StatusPending;
            
            var postId = await repo.CreateAsync(new CreatePostDto
            {
                Title = request.Title,
                Content = request.Content,
                IsPinned = request.IsPinned,
                IsAnnouncement = request.IsAnnouncement,
                Status = status
            }, userId, id, status);
            
            return Results.Created($"/api/posts/{postId}", new { id = postId, status });
        }).RequireAuthorization();

        app.MapPut("/api/posts/{id:guid}", async (Guid id, IPostRepository repo, UpdatePostRequest request, ClaimsPrincipal user) =>
        {
            var currentUserId = GetUserId(user);
            var currentUserRole = GetUserRole(user);
            var post = await repo.GetByIdAsync(id);
            
            if (post is null) return Results.NotFound();
            
            var isAuthor = post.AuthorId == currentUserId;
            var isAdminOrManager = currentUserRole == RoleAdministrator || currentUserRole == RoleManager;
            
            if (!isAuthor && !isAdminOrManager)
            {
                return Results.Forbid();
            }
            
            var updateDto = new UpdatePostDto
            {
                Title = request.Title,
                Content = request.Content,
                IsPinned = request.IsPinned,
                IsAnnouncement = request.IsAnnouncement
            };
            
            if (isAdminOrManager && request.Status.HasValue)
            {
                updateDto.Status = request.Status;
            }
            
            await repo.UpdateAsync(id, updateDto);
            return Results.Ok(new { message = "Publicación actualizada" });
        }).RequireAuthorization();

        app.MapDelete("/api/posts/{id:guid}", async (Guid id, IPostRepository repo, ClaimsPrincipal user) =>
        {
            var currentUserId = GetUserId(user);
            var currentUserRole = GetUserRole(user);
            var post = await repo.GetByIdAsync(id);
            
            if (post is null) return Results.NotFound();
            
            var isAuthor = post.AuthorId == currentUserId;
            var isAdmin = currentUserRole == RoleAdministrator;
            
            if (!isAuthor && !isAdmin)
            {
                return Results.Forbid();
            }
            
            await repo.SoftDeleteAsync(id);
            return Results.Ok(new { message = "Publicación eliminada" });
        }).RequireAuthorization();

        app.MapGet("/api/posts/{id:guid}/comments", async (Guid id, ICommentRepository repo) => Results.Ok(await repo.GetByPostAsync(id)))
            .RequireAuthorization();

        app.MapPost("/api/posts/{id:guid}/comments", async (Guid id, ICommentRepository repo, CreateCommentRequest request, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var commentId = await repo.CreateAsync(new CreateCommentDto
            {
                Content = request.Content,
                ParentCommentId = request.ParentCommentId
            }, userId, id);
            return Results.Created($"/api/comments/{commentId}", new { id = commentId });
        }).RequireAuthorization();

        app.MapDelete("/api/comments/{id:guid}", async (Guid id, ICommentRepository repo, ClaimsPrincipal user) =>
        {
            var currentUserId = GetUserId(user);
            var currentUserRole = GetUserRole(user);
            
            if (currentUserRole != RoleAdministrator)
            {
                return Results.Forbid();
            }
            
            await repo.SoftDeleteAsync(id);
            return Results.Ok(new { message = "Comentario eliminado" });
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
