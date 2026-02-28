using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.Repositories;
using UrbaPF.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["JWT_SECRET"] ?? "UrbaPFSuperSecretKey2026!ThisMustBeLongEnough";
var jwtIssuer = "UrbaPF";
var jwtAudience = "UrbaPF";

builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICondominiumRepository, CondominiumRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
builder.Services.AddScoped<IPollRepository, PollRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOneSignalService, OneSignalService>();
builder.Services.AddHttpClient<IOneSignalService, OneSignalService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim(ClaimTypes.Role, "0"));
    options.AddPolicy("ManagerOrAbove", policy => policy.RequireClaim(ClaimTypes.Role, "0", "1"));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "http://localhost:9000",
            "http://127.0.0.1:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => { Console.WriteLine("Health endpoint called"); return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }); });

// Public endpoints

app.Run();
app.MapPost("/api/auth/login", async (IAuthService authService, LoginRequest request) =>
{
    var (token, error) = await authService.LoginAsync(request.Email, request.Password);
    return token is not null 
        ? Results.Ok(new { token, message = "Login exitoso" })
        : Results.Unauthorized();
});

app.MapPost("/api/auth/register", async (IAuthService authService, RegisterRequest request) =>
{
    var (userId, error) = await authService.RegisterAsync(request.Email, request.Password, request.FullName, request.Phone);
    return userId.HasValue
        ? Results.Created($"/api/users/{userId}", new { id = userId, message = "Usuario registrado" })
        : Results.BadRequest(new { error });
});

// Protected endpoints - Users
app.MapGet("/api/users", async (IUserRepository repo) => Results.Ok(await repo.GetAllAsync()))
    .RequireAuthorization();
app.MapGet("/api/users/{id:guid}", async (Guid id, IUserRepository repo) => await repo.GetByIdAsync(id) is { } u ? Results.Ok(u) : Results.NotFound())
    .RequireAuthorization();
app.MapPost("/api/users", async (IUserRepository repo, CreateUserRequest request) =>
{
    var id = await repo.CreateAsync(new CreateUserDto { Email = request.Email, FullName = request.FullName, Phone = request.Phone }, request.Password);
    return Results.Created($"/api/users/{id}", new { id });
}).RequireAuthorization("AdminOnly");
app.MapPut("/api/users/{id:guid}", async (Guid id, IUserRepository repo, UpdateUserRequest request) =>
{
    await repo.UpdateAsync(id, new UpdateUserDto { FullName = request.FullName, Phone = request.Phone, FcmToken = request.FcmToken });
    return Results.Ok(new { message = "Usuario actualizado" });
}).RequireAuthorization();
app.MapDelete("/api/users/{id:guid}", async (Guid id, IUserRepository repo) => { await repo.SoftDeleteAsync(id); return Results.Ok(new { message = "Usuario eliminado" }); })
    .RequireAuthorization("AdminOnly");

// Protected endpoints - Condominiums
app.MapGet("/api/condominiums", async (ICondominiumRepository repo) => Results.Ok(await repo.GetAllAsync()))
    .RequireAuthorization();
app.MapGet("/api/condominiums/{id:guid}", async (Guid id, ICondominiumRepository repo) => await repo.GetByIdAsync(id) is { } c ? Results.Ok(c) : Results.NotFound())
    .RequireAuthorization();
app.MapPost("/api/condominiums", async (ICondominiumRepository repo, CreateCondominiumRequest request) =>
{
    var id = await repo.CreateAsync(new CreateCondominiumDto { Name = request.Name, Address = request.Address, Description = request.Description, Rules = request.Rules, MonthlyFee = request.MonthlyFee, Currency = request.Currency });
    return Results.Created($"/api/condominiums/{id}", new { id });
}).RequireAuthorization("AdminOnly");
app.MapPut("/api/condominiums/{id:guid}", async (Guid id, ICondominiumRepository repo, UpdateCondominiumRequest request) =>
{
    await repo.UpdateAsync(id, new UpdateCondominiumDto { Name = request.Name, Address = request.Address, Description = request.Description, Rules = request.Rules, MonthlyFee = request.MonthlyFee, IsActive = request.IsActive });
    return Results.Ok(new { message = "Condominio actualizado" });
}).RequireAuthorization("AdminOnly");
app.MapDelete("/api/condominiums/{id:guid}", async (Guid id, ICondominiumRepository repo) => { await repo.SoftDeleteAsync(id); return Results.Ok(new { message = "Condominio eliminado" }); })
    .RequireAuthorization("AdminOnly");

// Protected endpoints - Posts
app.MapGet("/api/condominiums/{id:guid}/posts", async (Guid id, IPostRepository repo) => Results.Ok(await repo.GetByCondominiumAsync(id)))
    .RequireAuthorization();
app.MapGet("/api/posts/{id:guid}", async (Guid id, IPostRepository repo) =>
{
    var post = await repo.GetByIdAsync(id);
    if (post is null) return Results.NotFound();
    await repo.IncrementViewCountAsync(id);
    return Results.Ok(post);
}).RequireAuthorization();
app.MapPost("/api/condominiums/{id:guid}/posts", async (Guid id, IPostRepository repo, CreatePostRequest request, HttpContext ctx) =>
{
    var userId = GetUserId(ctx);
    if (!userId.HasValue) return Results.Unauthorized();
    var postId = await repo.CreateAsync(new CreatePostDto { Title = request.Title, Content = request.Content, Category = request.Category, IsPinned = request.IsPinned, IsAnnouncement = request.IsAnnouncement }, userId.Value, id);
    return Results.Created($"/api/posts/{postId}", new { id = postId });
}).RequireAuthorization();
app.MapPut("/api/posts/{id:guid}", async (Guid id, IPostRepository repo, UpdatePostRequest request) =>
{
    await repo.UpdateAsync(id, new UpdatePostDto { Title = request.Title, Content = request.Content, Category = request.Category, IsPinned = request.IsPinned, IsAnnouncement = request.IsAnnouncement, Status = request.Status });
    return Results.Ok(new { message = "Publicación actualizada" });
}).RequireAuthorization();
app.MapDelete("/api/posts/{id:guid}", async (Guid id, IPostRepository repo) => { await repo.SoftDeleteAsync(id); return Results.Ok(new { message = "Publicación eliminada" }); })
    .RequireAuthorization();

// Protected endpoints - Comments
app.MapGet("/api/posts/{id:guid}/comments", async (Guid id, ICommentRepository repo) => Results.Ok(await repo.GetByPostAsync(id)))
    .RequireAuthorization();
app.MapPost("/api/posts/{id:guid}/comments", async (Guid id, ICommentRepository repo, CreateCommentRequest request, HttpContext ctx) =>
{
    var userId = GetUserId(ctx);
    if (!userId.HasValue) return Results.Unauthorized();
    var commentId = await repo.CreateAsync(new CreateCommentDto { ParentCommentId = request.ParentCommentId, Content = request.Content }, userId.Value, id);
    return Results.Created($"/api/comments/{commentId}", new { id = commentId });
}).RequireAuthorization();
app.MapDelete("/api/comments/{id:guid}", async (Guid id, ICommentRepository repo) => { await repo.SoftDeleteAsync(id); return Results.Ok(new { message = "Comentario eliminado" }); })
    .RequireAuthorization();

// Protected endpoints - Incidents
app.MapGet("/api/condominiums/{id:guid}/incidents", async (Guid id, IIncidentRepository repo, int? status) => Results.Ok(await repo.GetByCondominiumAsync(id, status)))
    .RequireAuthorization();
app.MapGet("/api/incidents/{id:guid}", async (Guid id, IIncidentRepository repo) => await repo.GetByIdAsync(id) is { } i ? Results.Ok(i) : Results.NotFound())
    .RequireAuthorization();
app.MapPost("/api/condominiums/{id:guid}/incidents", async (Guid id, IIncidentRepository repo, IOneSignalService oneSignal, CreateIncidentRequest request, HttpContext ctx) =>
{
    var userId = GetUserId(ctx);
    if (!userId.HasValue) return Results.Unauthorized();
    var incidentId = await repo.CreateAsync(new CreateIncidentDto { Title = request.Title, Description = request.Description, Type = request.Type, Priority = request.Priority, Latitude = request.Latitude, Longitude = request.Longitude, LocationDescription = request.LocationDescription, OccurredAt = request.OccurredAt }, userId.Value, id);
    await oneSignal.SendToSegmentAsync("Nuevo Incidente", request.Title, "All");
    return Results.Created($"/api/incidents/{incidentId}", new { id = incidentId });
}).RequireAuthorization();
app.MapPut("/api/incidents/{id:guid}/status", async (Guid id, IIncidentRepository repo, UpdateIncidentStatusRequest request) =>
{
    await repo.UpdateStatusAsync(id, request.Status, request.ResolutionNotes);
    return Results.Ok(new { message = "Estado actualizado" });
}).RequireAuthorization("ManagerOrAbove");
app.MapDelete("/api/incidents/{id:guid}", async (Guid id, IIncidentRepository repo) => { await repo.SoftDeleteAsync(id); return Results.Ok(new { message = "Incidente eliminado" }); })
    .RequireAuthorization("ManagerOrAbove");

// Protected endpoints - Polls
app.MapGet("/api/condominiums/{id:guid}/polls", async (Guid id, IPollRepository repo) => Results.Ok(await repo.GetByCondominiumAsync(id)))
    .RequireAuthorization();
app.MapGet("/api/polls/{id:guid}", async (Guid id, IPollRepository repo) => await repo.GetByIdAsync(id) is { } p ? Results.Ok(p) : Results.NotFound())
    .RequireAuthorization();
app.MapPost("/api/condominiums/{id:guid}/polls", async (Guid id, IPollRepository repo, IOneSignalService oneSignal, CreatePollRequest request, HttpContext ctx) =>
{
    var userId = GetUserId(ctx);
    if (!userId.HasValue) return Results.Unauthorized();
    var pollId = await repo.CreateAsync(new CreatePollDto { Title = request.Title, Description = request.Description, Options = request.Options, PollType = request.PollType, StartsAt = request.StartsAt, EndsAt = request.EndsAt, RequiresJustification = request.RequiresJustification, MinRoleToVote = request.MinRoleToVote }, userId.Value, id);
    await oneSignal.SendToSegmentAsync("Nueva Votación", request.Title, "All");
    return Results.Created($"/api/polls/{pollId}", new { id = pollId });
}).RequireAuthorization("ManagerOrAbove");
app.MapPut("/api/polls/{id:guid}", async (Guid id, IPollRepository repo, UpdatePollRequest request) =>
{
    await repo.UpdateAsync(id, new UpdatePollDto { Title = request.Title, Description = request.Description, Options = request.Options, StartsAt = request.StartsAt, EndsAt = request.EndsAt, Status = request.Status });
    return Results.Ok(new { message = "Votación actualizada" });
}).RequireAuthorization("ManagerOrAbove");
app.MapDelete("/api/polls/{id:guid}", async (Guid id, IPollRepository repo) => { await repo.SoftDeleteAsync(id); return Results.Ok(new { message = "Votación eliminada" }); })
    .RequireAuthorization("ManagerOrAbove");

// Protected endpoints - Votes
app.MapPost("/api/polls/{id:guid}/vote", async (Guid id, IPollRepository pollRepo, IVoteRepository voteRepo, CreateVoteRequest request, HttpContext ctx) =>
{
    var userId = GetUserId(ctx);
    if (!userId.HasValue) return Results.Unauthorized();
    if (await voteRepo.HasUserVotedAsync(id, userId.Value)) return Results.BadRequest(new { error = "Ya has votado" });
    var poll = await pollRepo.GetByIdAsync(id);
    if (poll == null) return Results.NotFound();
    var signature = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes($"{userId}:{request.OptionIndex}:{DateTime.UtcNow:O}:secret")));
    var voteId = await voteRepo.CreateAsync(userId.Value, id, request.OptionIndex, signature, ctx.Connection.RemoteIpAddress?.ToString() ?? "");
    return Results.Created($"/api/votes/{voteId}", new { id = voteId, message = "Voto registrado" });
}).RequireAuthorization();
app.MapGet("/api/polls/{id:guid}/votes", async (Guid id, IVoteRepository repo) => Results.Ok(new { votes = await repo.GetByPollAsync(id), results = await repo.GetResultsAsync(id) }))
    .RequireAuthorization();

// Protected endpoints - Alerts
app.MapGet("/api/condominiums/{id:guid}/alerts", async (Guid id, IAlertRepository repo) => Results.Ok(await repo.GetActiveByCondominiumAsync(id)))
    .RequireAuthorization();
app.MapGet("/api/alerts/{id:guid}", async (Guid id, IAlertRepository repo) => await repo.GetByIdAsync(id) is { } a ? Results.Ok(a) : Results.NotFound())
    .RequireAuthorization();
app.MapPost("/api/condominiums/{id:guid}/alerts", async (Guid id, IAlertRepository repo, IOneSignalService oneSignal, CreateAlertRequest request, HttpContext ctx) =>
{
    var userId = GetUserId(ctx);
    if (!userId.HasValue) return Results.Unauthorized();
    var alertId = await repo.CreateAsync(new CreateAlertDto { AlertType = request.AlertType, Message = request.Message, Latitude = request.Latitude, Longitude = request.Longitude, DestinationAddress = request.DestinationAddress, EstimatedArrival = request.EstimatedArrival }, userId.Value, id);
    await oneSignal.SendToSegmentAsync("Alerta", request.Message, "All");
    return Results.Created($"/api/alerts/{alertId}", new { id = alertId });
}).RequireAuthorization();
app.MapPut("/api/alerts/{id:guid}/status", async (Guid id, IAlertRepository repo, UpdateAlertStatusRequest request) =>
{
    await repo.UpdateStatusAsync(id, request.Status);
    return Results.Ok(new { message = "Estado actualizado" });
}).RequireAuthorization();

Guid? GetUserId(HttpContext ctx)
{
    var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
}

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FullName, string? Phone);
public record CreateUserRequest(string Email, string Password, string FullName, string? Phone);
public record UpdateUserRequest(string? FullName, string? Phone, string? FcmToken);
public record CreateCondominiumRequest(string Name, string Address, string? Description, string? Rules, decimal MonthlyFee, string? Currency);
public record UpdateCondominiumRequest(string? Name, string? Address, string? Description, string? Rules, decimal? MonthlyFee, bool? IsActive);
public record CreatePostRequest(string Title, string Content, int Category, bool IsPinned, bool IsAnnouncement);
public record UpdatePostRequest(string? Title, string? Content, int? Category, bool? IsPinned, bool? IsAnnouncement, int? Status);
public record CreateCommentRequest(Guid? ParentCommentId, string Content);
public record CreateIncidentRequest(string Title, string Description, int Type, int Priority, double? Latitude, double? Longitude, string? LocationDescription, DateTime OccurredAt);
public record UpdateIncidentStatusRequest(int Status, string? ResolutionNotes);
public record CreatePollRequest(string Title, string? Description, string Options, int PollType, DateTime StartsAt, DateTime EndsAt, bool RequiresJustification, int MinRoleToVote);
public record UpdatePollRequest(string? Title, string? Description, string? Options, DateTime? StartsAt, DateTime? EndsAt, int? Status);
public record CreateVoteRequest(int OptionIndex);
public record CreateAlertRequest(int AlertType, string Message, double? Latitude, double? Longitude, string? DestinationAddress, DateTime EstimatedArrival);
public record UpdateAlertStatusRequest(int Status);
