using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.Repositories;
using UrbaPF.Infrastructure.Services;
using UrbaPF.Domain.Services;
using UrbaPF.Api.DTOs;
using UrbaPF.Api.Routes;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FluentMigrator.Runner;
using UrbaPF.Infrastructure.Migrations;
using UrbaPF.Api.Extensions;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

// Initialize Firebase Admin
var firebaseCredPath = builder.Configuration["FIREBASE_SERVICE_ACCOUNT_PATH"] ?? "/app/firebase-service-account.json";
if (File.Exists(firebaseCredPath))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebaseCredPath)
    });
    Console.WriteLine("[Firebase] Admin SDK initialized successfully.");
}
else
{
    var firebaseJson = builder.Configuration["FIREBASE_SERVICE_ACCOUNT_JSON"];
    if (!string.IsNullOrEmpty(firebaseJson))
    {
        try
        {
            var credential = GoogleCredential.FromJson(firebaseJson);
            FirebaseApp.Create(new AppOptions
            {
                Credential = credential
            });
            Console.WriteLine("[Firebase] Admin SDK initialized from JSON.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Firebase] Failed to initialize from JSON: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("[Firebase] Not configured. Push notifications will be disabled.");
    }
}

var jwtSecret = builder.Configuration["JWT_SECRET"] ?? "UrbaPFSuperSecretKey2026!ThisMustBeLongEnough";
var jwtIssuer = "UrbaPF";
var jwtAudience = "UrbaPF";

builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<DbConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICondominiumRepository, CondominiumRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IPollRepository, PollRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
builder.Services.AddScoped<IIncidentService, IncidentService>();
builder.Services.AddScoped<IncidentDomainService>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<AlertDomainService>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<Func<DateTime>>(() => DateTime.UtcNow);
builder.Services.AddHttpClient<IPushNotificationService, PushNotificationService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();

// Build connection string from environment variables
var host = builder.Configuration["DB_HOST"] ?? "localhost";
var port = builder.Configuration["DB_PORT"] ?? "5432";
var database = builder.Configuration["DB_NAME"] ?? "urbapf";
var user = builder.Configuration["DB_USER"] ?? "postgres";
var password = builder.Configuration["DB_PASSWORD"] ?? "postgres";
var connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}";

// Configure FluentMigrator for data migrations only
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(M001_CreateCondominiumsTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

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
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
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

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Run migrations
app.RunMigrations();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Map routes
app.MapAuthRoutes();
app.MapUserRoutes();
app.MapCondominiumRoutes();
app.MapPostRoutes();
app.MapPollRoutes();
app.MapIncidentRoutes();
app.MapAlertRoutes();
app.MapExpenseRoutes();

app.Run();
