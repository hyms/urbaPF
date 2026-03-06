using Testcontainers.PostgreSql;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Npgsql;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.Repositories;
using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Tests.Integration.Repositories;

public class RepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:15-3.4")
        .WithDatabase("urbapf_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private string ConnectionString => _postgres.GetConnectionString();
    private IConfiguration _configuration = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        
        var inMemorySettings = new Dictionary<string, string?> {
            {"DB_HOST", "localhost"},
            {"DB_PORT", _postgres.GetMappedPublicPort(5432).ToString()},
            {"DB_NAME", "urbapf_test"},
            {"DB_USER", "test"},
            {"DB_PASSWORD", "test"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        await InitializeDatabase();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    private async Task InitializeDatabase()
    {
        var factory = new DbConnectionFactory(_configuration);
        using var conn = factory.CreateConnection();
        conn.Open();
        
        var sql = @"
            CREATE EXTENSION IF NOT EXISTS postgis;
            
            CREATE TABLE IF NOT EXISTS condominiums (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(255) NOT NULL,
                address VARCHAR(500),
                logo_url VARCHAR(1000),
                description TEXT,
                rules TEXT,
                monthly_fee DECIMAL(10,2) DEFAULT 0,
                currency VARCHAR(10) DEFAULT 'BOB',
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP,
                is_active BOOLEAN DEFAULT true,
                deleted_at TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS users (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                email VARCHAR(255) UNIQUE NOT NULL,
                password_hash VARCHAR(255) NOT NULL,
                full_name VARCHAR(255) NOT NULL,
                phone VARCHAR(50),
                photo_url VARCHAR(500),
                role INTEGER DEFAULT 1,
                credibility_level INTEGER DEFAULT 1,
                status INTEGER DEFAULT 1,
                condominium_id UUID REFERENCES condominiums(id),
                lot_number VARCHAR(50),
                street_address VARCHAR(500),
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                last_login_at TIMESTAMP,
                is_validated BOOLEAN DEFAULT false,
                manager_votes INTEGER DEFAULT 0,
                deleted_at TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS posts (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                condominium_id UUID NOT NULL REFERENCES condominiums(id),
                author_id UUID NOT NULL REFERENCES users(id),
                title VARCHAR(500) NOT NULL,
                content TEXT NOT NULL,
                category INTEGER DEFAULT 1,
                is_pinned BOOLEAN DEFAULT false,
                is_announcement BOOLEAN DEFAULT false,
                status INTEGER DEFAULT 1,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP,
                view_count INTEGER DEFAULT 0,
                deleted_at TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS incidents (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                condominium_id UUID NOT NULL REFERENCES condominiums(id),
                reported_by_id UUID NOT NULL REFERENCES users(id),
                title VARCHAR(500) NOT NULL,
                description TEXT,
                type INTEGER DEFAULT 1,
                priority INTEGER DEFAULT 1,
                status INTEGER DEFAULT 1,
                latitude DOUBLE PRECISION,
                longitude DOUBLE PRECISION,
                location_description VARCHAR(500),
                occurred_at TIMESTAMP NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP,
                resolved_at TIMESTAMP,
                resolution_notes TEXT,
                deleted_at TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS polls (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                condominium_id UUID NOT NULL REFERENCES condominiums(id),
                title VARCHAR(500) NOT NULL,
                description TEXT,
                options JSONB DEFAULT '[]'::jsonb,
                poll_type INTEGER DEFAULT 1,
                starts_at TIMESTAMP NOT NULL,
                ends_at TIMESTAMP NOT NULL,
                requires_justification BOOLEAN DEFAULT false,
                min_role_to_vote INTEGER DEFAULT 1,
                server_secret VARCHAR(255) NOT NULL DEFAULT 'default-secret',
                status INTEGER DEFAULT 1,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP,
                created_by_id UUID NOT NULL REFERENCES users(id),
                deleted_at TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS votes (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                poll_id UUID NOT NULL REFERENCES polls(id),
                user_id UUID NOT NULL REFERENCES users(id),
                option_index INTEGER NOT NULL,
                digital_signature VARCHAR(255),
                voted_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                ip_address VARCHAR(50),
                deleted_at TIMESTAMP,
                UNIQUE(poll_id, user_id)
            );

            CREATE TABLE IF NOT EXISTS alerts (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                condominium_id UUID NOT NULL REFERENCES condominiums(id),
                created_by_id UUID NOT NULL REFERENCES users(id),
                alert_type INTEGER DEFAULT 1,
                message TEXT NOT NULL,
                latitude DOUBLE PRECISION,
                longitude DOUBLE PRECISION,
                destination_address VARCHAR(500),
                estimated_arrival TIMESTAMP NOT NULL,
                status INTEGER DEFAULT 1,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP,
                acknowledged_at TIMESTAMP,
                arrived_at TIMESTAMP,
                deleted_at TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS comments (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                post_id UUID NOT NULL REFERENCES posts(id),
                author_id UUID NOT NULL REFERENCES users(id),
                parent_comment_id UUID REFERENCES comments(id),
                content TEXT NOT NULL,
                credibility_level INTEGER DEFAULT 1,
                is_hidden BOOLEAN DEFAULT false,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                deleted_at TIMESTAMP
            );
        ";
        
        using var cmd = new NpgsqlCommand(sql, (NpgsqlConnection)conn);
        await cmd.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task CondominiumRepository_Create_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var repo = new CondominiumRepository(factory);

        var dto = new CreateCondominiumDto
        {
            Name = "Test Condo",
            Address = "Test Address",
            Description = "Description",
            MonthlyFee = 500.00m,
            Currency = "BOB"
        };

        var id = await repo.CreateAsync(dto);

        id.Should().NotBe(Guid.Empty);

        var result = await repo.GetByIdAsync(id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Condo");
    }

    [Fact]
    public async Task CondominiumRepository_GetAll_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var repo = new CondominiumRepository(factory);

        var dto1 = new CreateCondominiumDto { Name = "Condo 1", Address = "Address 1", MonthlyFee = 100m };
        var dto2 = new CreateCondominiumDto { Name = "Condo 2", Address = "Address 2", MonthlyFee = 200m };

        await repo.CreateAsync(dto1);
        await repo.CreateAsync(dto2);

        var results = await repo.GetAllAsync();

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task CondominiumRepository_Update_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var repo = new CondominiumRepository(factory);
        
        var id = await repo.CreateAsync(new CreateCondominiumDto { Name = "Original", Address = "Address", MonthlyFee = 100 });

        await repo.UpdateAsync(id, new UpdateCondominiumDto { Name = "Updated", MonthlyFee = 200 });

        var result = await repo.GetByIdAsync(id);
        result!.Name.Should().Be("Updated");
        result.MonthlyFee.Should().Be(200);
    }

    [Fact]
    public async Task CondominiumRepository_SoftDelete_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var repo = new CondominiumRepository(factory);
        
        var id = await repo.CreateAsync(new CreateCondominiumDto { Name = "ToDelete", Address = "Address", MonthlyFee = 100 });

        await repo.SoftDeleteAsync(id);

        var result = await repo.GetByIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task UserRepository_Create_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var condoRepo = new CondominiumRepository(factory);
        var condoId = await condoRepo.CreateAsync(new CreateCondominiumDto { Name = "Test", Address = "Test", MonthlyFee = 100 });

        var repo = new UserRepository(factory);
        
        var dto = new CreateUserDto
        {
            Email = "new@test.com",
            FullName = "New User",
            Phone = "12345678",
            CondominiumId = condoId
        };

        var id = await repo.CreateAsync(dto, "hashed_password");

        id.Should().NotBe(Guid.Empty);

        var result = await repo.GetByIdAsync(id);
        result.Should().NotBeNull();
        result!.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task UserRepository_GetByEmail_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var repo = new UserRepository(factory);
        
        var dto = new CreateUserDto { Email = "findme@test.com", FullName = "Find Me" };
        var id = await repo.CreateAsync(dto, "hash");

        var result = await repo.GetByEmailAsync("findme@test.com");

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task PostRepository_Create_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var condoRepo = new CondominiumRepository(factory);
        var condoId = await condoRepo.CreateAsync(new CreateCondominiumDto { Name = "Test", Address = "Test", MonthlyFee = 100 });
        
        var userRepo = new UserRepository(factory);
        var userId = await userRepo.CreateAsync(new CreateUserDto { Email = "a@a.com", FullName = "Author" }, "hash");

        var repo = new PostRepository(factory);
        
        var dto = new CreatePostDto
        {
            Title = "Test Post",
            Content = "Test Content",
            Category = 1,
            IsPinned = false,
            IsAnnouncement = false
        };

        var id = await repo.CreateAsync(dto, userId, condoId);

        id.Should().NotBe(Guid.Empty);

        var result = await repo.GetByIdAsync(id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Post");
    }

    [Fact]
    public async Task PostRepository_GetByCondominium_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var condoRepo = new CondominiumRepository(factory);
        var condoId = await condoRepo.CreateAsync(new CreateCondominiumDto { Name = "Test", Address = "Test", MonthlyFee = 100 });
        
        var userRepo = new UserRepository(factory);
        var userId = await userRepo.CreateAsync(new CreateUserDto { Email = "b@b.com", FullName = "Author" }, "hash");

        var postRepo = new PostRepository(factory);
        await postRepo.CreateAsync(new CreatePostDto { Title = "Post 1", Content = "Content 1", Category = 1 }, userId, condoId);
        await postRepo.CreateAsync(new CreatePostDto { Title = "Post 2", Content = "Content 2", Category = 1 }, userId, condoId);

        var results = await postRepo.GetByCondominiumAsync(condoId);

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task PostRepository_Update_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var condoRepo = new CondominiumRepository(factory);
        var condoId = await condoRepo.CreateAsync(new CreateCondominiumDto { Name = "Test", Address = "Test", MonthlyFee = 100 });
        
        var userRepo = new UserRepository(factory);
        var userId = await userRepo.CreateAsync(new CreateUserDto { Email = "c@c.com", FullName = "Author" }, "hash");

        var repo = new PostRepository(factory);
        var id = await repo.CreateAsync(new CreatePostDto { Title = "Original", Content = "Content", Category = 1 }, userId, condoId);

        await repo.UpdateAsync(id, new UpdatePostDto { Title = "Updated", IsPinned = true });

        var result = await repo.GetByIdAsync(id);
        result!.Title.Should().Be("Updated");
        result.IsPinned.Should().BeTrue();
    }

    [Fact]
    public async Task IncidentRepository_Create_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var condoRepo = new CondominiumRepository(factory);
        var condoId = await condoRepo.CreateAsync(new CreateCondominiumDto { Name = "Test", Address = "Test", MonthlyFee = 100 });
        
        var userRepo = new UserRepository(factory);
        var userId = await userRepo.CreateAsync(new CreateUserDto { Email = "d@d.com", FullName = "Reporter" }, "hash");

        var repo = new IncidentRepository(factory);
        
        var dto = new CreateIncidentDto
        {
            Title = "Test Incident",
            Description = "Description",
            Type = 1,
            Priority = 2,
            Latitude = -17.8146,
            Longitude = -63.1561,
            OccurredAt = DateTime.UtcNow
        };

        var id = await repo.CreateAsync(dto, userId, condoId);

        id.Should().NotBe(Guid.Empty);

        var result = await repo.GetByIdAsync(id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Incident");
    }

    [Fact]
    public async Task IncidentRepository_GetByCondominium_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var condoRepo = new CondominiumRepository(factory);
        var condoId = await condoRepo.CreateAsync(new CreateCondominiumDto { Name = "Test", Address = "Test", MonthlyFee = 100 });
        
        var userRepo = new UserRepository(factory);
        var userId = await userRepo.CreateAsync(new CreateUserDto { Email = "e@e.com", FullName = "Reporter" }, "hash");

        var repo = new IncidentRepository(factory);
        await repo.CreateAsync(new CreateIncidentDto { Title = "Incident 1", Description = "Desc", Type = 1, Priority = 1, OccurredAt = DateTime.UtcNow }, userId, condoId);
        await repo.CreateAsync(new CreateIncidentDto { Title = "Incident 2", Description = "Desc", Type = 1, Priority = 2, OccurredAt = DateTime.UtcNow }, userId, condoId);

        var results = await repo.GetByCondominiumAsync(condoId);

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task PollRepository_Create_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var condoRepo = new CondominiumRepository(factory);
        var condoId = await condoRepo.CreateAsync(new CreateCondominiumDto { Name = "Test", Address = "Test", MonthlyFee = 100 });
        
        var userRepo = new UserRepository(factory);
        var userId = await userRepo.CreateAsync(new CreateUserDto { Email = "f@f.com", FullName = "Creator" }, "hash");

        var repo = new PollRepository(factory);
        
        var dto = new CreatePollDto
        {
            Title = "Test Poll",
            Description = "Description",
            Options = "[\"Option 1\",\"Option 2\"]",
            PollType = 1,
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(7),
            RequiresJustification = false,
            MinRoleToVote = 1
        };

        var id = await repo.CreateAsync(dto, userId, condoId);

        id.Should().NotBe(Guid.Empty);

        var result = await repo.GetByIdAsync(id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Poll");
    }

    [Fact]
    public async Task VoteRepository_Create_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var condoRepo = new CondominiumRepository(factory);
        var condoId = await condoRepo.CreateAsync(new CreateCondominiumDto { Name = "Test", Address = "Test", MonthlyFee = 100 });
        
        var userRepo = new UserRepository(factory);
        var userId = await userRepo.CreateAsync(new CreateUserDto { Email = "g@g.com", FullName = "Voter" }, "hash");

        var pollRepo = new PollRepository(factory);
        var pollId = await pollRepo.CreateAsync(new CreatePollDto
        {
            Title = "Poll",
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(7)
        }, userId, condoId);

        var repo = new VoteRepository(factory);
        
        var id = await repo.CreateAsync(userId, pollId, 1, "signature", "127.0.0.1");

        id.Should().NotBe(Guid.Empty);

        var hasVoted = await repo.HasUserVotedAsync(pollId, userId);
        hasVoted.Should().BeTrue();
    }

    [Fact]
    public async Task VoteRepository_GetResults_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var condoRepo = new CondominiumRepository(factory);
        var condoId = await condoRepo.CreateAsync(new CreateCondominiumDto { Name = "Test", Address = "Test", MonthlyFee = 100 });
        
        var userRepo = new UserRepository(factory);
        var userId1 = await userRepo.CreateAsync(new CreateUserDto { Email = "h@h.com", FullName = "Voter1" }, "hash");
        var userId2 = await userRepo.CreateAsync(new CreateUserDto { Email = "i@i.com", FullName = "Voter2" }, "hash");

        var pollRepo = new PollRepository(factory);
        var pollId = await pollRepo.CreateAsync(new CreatePollDto
        {
            Title = "Poll",
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(7)
        }, userId1, condoId);

        var voteRepo = new VoteRepository(factory);
        await voteRepo.CreateAsync(userId1, pollId, 0, "sig1", "ip1");
        await voteRepo.CreateAsync(userId2, pollId, 1, "sig2", "ip2");

        var results = await voteRepo.GetResultsAsync(pollId);

        results.Should().ContainKey(0);
        results.Should().ContainKey(1);
    }

    [Fact]
    public async Task AlertRepository_Create_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var condoRepo = new CondominiumRepository(factory);
        var condoId = await condoRepo.CreateAsync(new CreateCondominiumDto { Name = "Test", Address = "Test", MonthlyFee = 100 });
        
        var userRepo = new UserRepository(factory);
        var userId = await userRepo.CreateAsync(new CreateUserDto { Email = "j@j.com", FullName = "Creator" }, "hash");

        var repo = new AlertRepository(factory);
        
        var dto = new CreateAlertDto
        {
            AlertType = 1,
            Message = "Security Alert",
            EstimatedArrival = DateTime.UtcNow.AddMinutes(30)
        };

        var id = await repo.CreateAsync(dto, userId, condoId);

        id.Should().NotBe(Guid.Empty);

        var result = await repo.GetByIdAsync(id);
        result.Should().NotBeNull();
        result!.Message.Should().Be("Security Alert");
    }

    [Fact]
    public async Task AlertRepository_GetActive_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var condoRepo = new CondominiumRepository(factory);
        var condoId = await condoRepo.CreateAsync(new CreateCondominiumDto { Name = "Test", Address = "Test", MonthlyFee = 100 });
        
        var userRepo = new UserRepository(factory);
        var userId = await userRepo.CreateAsync(new CreateUserDto { Email = "k@k.com", FullName = "Creator" }, "hash");

        var repo = new AlertRepository(factory);
        await repo.CreateAsync(new CreateAlertDto { AlertType = 1, Message = "Alert 1", EstimatedArrival = DateTime.UtcNow }, userId, condoId);
        await repo.CreateAsync(new CreateAlertDto { AlertType = 2, Message = "Alert 2", EstimatedArrival = DateTime.UtcNow }, userId, condoId);

        var results = await repo.GetActiveByCondominiumAsync(condoId);

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task CommentRepository_Create_Succeeds()
    {
        var factory = new DbConnectionFactory(_configuration);
        var condoRepo = new CondominiumRepository(factory);
        var condoId = await condoRepo.CreateAsync(new CreateCondominiumDto { Name = "Test", Address = "Test", MonthlyFee = 100 });
        
        var userRepo = new UserRepository(factory);
        var userId = await userRepo.CreateAsync(new CreateUserDto { Email = "l@l.com", FullName = "Author" }, "hash");

        var postRepo = new PostRepository(factory);
        var postId = await postRepo.CreateAsync(new CreatePostDto { Title = "Post", Content = "Content", Category = 1 }, userId, condoId);

        var repo = new CommentRepository(factory);
        
        var dto = new CreateCommentDto
        {
            Content = "Test Comment",
            ParentCommentId = null
        };

        var id = await repo.CreateAsync(dto, userId, postId);

        id.Should().NotBe(Guid.Empty);

        var results = await repo.GetByPostAsync(postId);
        results.Should().HaveCount(1);
    }
}
