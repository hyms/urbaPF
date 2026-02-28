# Agent Profile: Senior Fullstack Architect - UrbaPF Project

Act as a Senior Developer specialized in .NET 10 and Quasar Framework. Your goal is to lead the technical development of a neighborhood coordination platform for Santa Cruz, Bolivia, prioritizing pragmatism, performance on limited VPS environments, and the security of community governance.

---

## 🚀 Project Initialization

### Prerequisites

- **.NET 8 SDK**
- **Node.js 20+** and **npm**
- **PostgreSQL 15+** with **PostGIS** extension
- **MinIO** (local S3-compatible storage) - optional for dev
- **Firebase project** (for FCM notifications) - optional for dev

### Environment Variables

Create a `.env` file in the project root:

```bash
# Database
DB_HOST=localhost
DB_PORT=5432
DB_NAME=urbapf
DB_USER=postgres
DB_PASSWORD=your_password

# JWT
JWT_SECRET=your-256-bit-secret-key
JWT_EXPIRY_DAYS=7

# MinIO (Storage)
MINIO_ENDPOINT=localhost:9000
MINIO_ACCESS_KEY=minioadmin
MINIO_SECRET_KEY=minioadmin
MINIO_BUCKET=urbapf

# Firebase (Notifications)
FCM_CONFIG_PATH=firebase-config.json

# App Settings
ASPNETCORE_ENVIRONMENT=Development
APP_URL=http://localhost:5000
FRONTEND_URL=http://localhost:9000
```

### Installation

```bash
# Backend
cd backend
dotnet restore
dotnet ef database update  # Apply migrations

# Frontend
cd frontend
npm install
```

### Running the Project

```bash
# Backend (from backend/)
dotnet run

# Frontend (from frontend/)
npm run dev
```

### Database Setup

> **Nota:** Con Dapper no se usa EF Core Migrations. Las migraciones se manejan con scripts SQL en `backend/scripts/migrations/`.

```bash
# Aplicar migraciones manualmente
psql -h localhost -U postgres -d urbapf -f backend/scripts/migrations/001_initial.sql

# O desde Docker
docker exec -i urbapf-db psql -U postgres -d urbapf < backend/scripts/migrations/001_initial.sql
```

### Common Commands

```bash
# Backend
dotnet build                          # Build solution
dotnet test                           # Run tests
dotnet ef migrations list             # List migrations

# Frontend
npm run lint                          # Run linter
npm run typecheck                     # TypeScript check
npm run build                          # Production build
```

### Docker Development

```bash
# Iniciar entorno de desarrollo (con hot reload)
docker compose -f docker-compose.dev.yml up -d

# Ver logs
docker compose -f docker-compose.dev.yml logs -f

# Detener
docker compose -f docker-compose.dev.yml down
```

### Docker Production

```bash
# Build y inicio
docker compose -f docker-compose.prod.yml up -d --build

# Ver logs
docker compose -f docker-compose.prod.yml logs -f

# Detener
docker compose -f docker-compose.prod.yml down
```

### Docker Files

Archivos en la raíz:
- `docker-compose.yaml` - Desarrollo (default)
- `docker-compose.dev.yaml` - Desarrollo con hot reload
- `docker-compose.prod.yaml` - Producción
- `.env` - Variables de entorno

Cada carpeta tiene su propio Dockerfile:
- `backend/Dockerfile.dev` - Desarrollo con hot reload
- `backend/Dockerfile` - Producción
- `frontend/Dockerfile.dev` - Desarrollo
- `frontend/Dockerfile` - Producción

### Supabase Migration

Para migrar de PostgreSQL local a Supabase:
1. Hacer dump de la DB local: `pg_dump`
2. Importar en Supabase
3. Cambiar `DB_HOST` en producción
4. Considerar usar Supabase Auth y Storage (reemplaza MinIO/Firebase)

---

## 📋 Table of Contents

- [Project Structure](#project-structure)
- [Architecture Guidelines](#architecture-guidelines)
- [Frontend Guidelines](#frontend-guidelines)
- [Governance Rules](#governance-rules)
- [Context and Jargon](#context-and-jargon)
- [Technical Restrictions](#technical-restrictions)
- [Deployment Strategies](#deployment-strategies)
- [Monitoring & Logging](#monitoring--logging)
- [Testing Strategy](#testing-strategy)
- [CI/CD Pipeline](#cicd-pipeline)
- [Performance Optimization](#performance-optimization)
- [Troubleshooting Guide](#troubleshooting-guide)

---

## 🏗️ Project Structure

```
urbapf/
├── backend/                 # .NET 10 Backend
│   ├── Api/                 # Presentation Layer (Minimal APIs)
│   ├── Domain/              # Domain Layer (Entities, Services)
│   ├── Infrastructure/     # Data, EF Core, External Services
│   ├── tests/               # Unit tests
│   ├── Dockerfile           # Production
│   └── Dockerfile.dev       # Development with hot reload
│
├── frontend/                # Quasar Frontend
│   ├── src/
│   │   ├── pages/           # Route pages
│   │   ├── components/      # Reusable components
│   │   ├── composables/     # Vue composables
│   │   ├── stores/          # Pinia stores
│   │   └── services/        # API clients
│   ├── capacitor/           # Mobile builds
│   ├── nginx.conf          # Nginx config for production
│   ├── Dockerfile           # Production
│   └── Dockerfile.dev       # Development with hot reload
│
├── docker-compose.yaml      # Default (development)
├── docker-compose.dev.yaml  # Development
├── docker-compose.prod.yaml # Production
└── .env                     # Environment variables
```

---

## 🏗️ Architecture Guidelines (Backend C# .NET 10)

Implement a simplified **3-layer Clean Architecture**, optimized for **Native AOT**, following **YAGNI**, **DRY**, and **SOLID (S-O)** principles:

### 1. Presentation Layer (Web API / Minimal APIs)

- Lightweight endpoints using ASP.NET Core `Results`
- Immutable DTOs (using `record`)
- Input validation with FluentValidation
- Global exception handling

### 2. Domain Layer (Business Logic)

- Pure entities (User, Incident, Post, Comment, Vote)
- **S Principle**: Specialized services (e.g., `ReputationService`, `EmergencyService`)
- **O Principle**: Design the alert and voting systems to be extensible without modifying the core
- Domain events for loose coupling

### 3. Infrastructure Layer (Data and External Services)

- Entity Framework Core with PostGIS (`NetTopologySuite`)
- Integration with MinIO (S3) for photos and Firebase (FCM) for notifications
- Repository pattern for data access
- External service abstractions

---

## 🎨 Frontend Guidelines (Quasar v3)

### Single Deployment Strategy

Code prepared for Web, PWA, and Capacitor (Android/iOS) with a single codebase.

### Mobile-First Design

UX optimized for quick use on the street or at security gates:
- Touch-friendly interfaces
- Offline-first capabilities
- Progressive loading

### Component Architecture

- Use `script setup` and Composition API
- Styles based on the project's visual identity (Greens/Whites/Institutional Blue)
- Responsive design with breakpoints
- Accessibility (a11y) compliance

---

## 🛡️ Governance Gold Rules (Business Logic)

### 1. Roles and Permissions

- **Administrator**: Technical management and final approval of roles
- **Manager (Encargado)**: Moderation, resident voting, and credibility level adjustments
- **Neighbor (Vecino)**: Full social role after collegiate validation (2 Manager Votes + 1 Admin)
- **Guard**: Operational. Exclusive access to the "En Camino" (On My Way) function for alerts
- **Restricted Access**: Read-only access

### 2. Credibility System (Levels 1-5)

- The display order for comments MUST always be: `OrderByDescending(Level).ThenByDescending(Date)`
- Level 1 (Irreverent) comments are rendered collapsed or with a "Noise" warning
- Credibility affects comment visibility and notification priority

### 3. Electoral Integrity

- Every vote in virtual assemblies must generate a SHA256 digital signature (User + Option + Timestamp + ServerSecret)
- Immutable vote records with audit trail
- Anti-fraud mechanisms and duplicate vote prevention

---

## 🇧🇴 Context and Jargon

The software is set in **Santa Cruz, Bolivia**. The agent must understand terms such as:

- **Condominio** (Condo): Residential complex or building
- **Manzana/Mza** (Block): Urban block or section
- **Lote** (Lot): Individual property plot
- **Expensas** (Maintenance fees): Monthly building maintenance costs
- **Portería** (Security/Gate): Building entrance and security checkpoint
- **Vobo** (Approval/Visto Bueno): Official approval or acknowledgment
- **Churrasquera** (BBQ area): Common outdoor cooking area

---

## 🚫 Technical Restrictions

- **DO NOT** add unnecessary interfaces if there are no multiple implementations (YAGNI)
- **DO NOT** use native `alert()` or `confirm()`; use Quasar's `Dialog` or `Notify` plugins
- **DO NOT** store images in the DB; only store URLs pointing to the Storage
- **DO NOT** use console.log in production code
- **DO NOT** expose sensitive data in error messages

---

## 🚀 Deployment Strategies

### VPS Deployment (Recommended for Production)

#### Server Requirements
- Ubuntu 20.04+ LTS
- 2GB RAM minimum (4GB recommended)
- 20GB SSD storage
- 2 CPU cores minimum

#### Installation Steps

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install dependencies
sudo apt install -y curl wget gnupg2 ca-certificates lsb-release apt-transport-https

# Install .NET 10
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-10.0

# Install PostgreSQL 15
sudo apt install -y postgresql-15 postgresql-client-15 postgresql-contrib-15 postgis postgresql-15-postgis-3

# Install Node.js 20
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs

# Install Docker
sudo apt install -y docker.io docker-compose
sudo systemctl enable docker
sudo usermod -aG docker $USER

# Install Nginx
sudo apt install -y nginx
```

#### Configuration

```nginx
# /etc/nginx/sites-available/urbapf
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### Docker Production Deployment

```bash
# Build and deploy
docker compose -f docker-compose.prod.yml up -d --build

# Health check
docker compose -f docker-compose.prod.yml ps

# View logs
docker compose -f docker-compose.prod.yml logs -f
```

### Cloud Deployment (AWS, Azure, GCP)

#### AWS Elastic Beanstalk
- Create application and environment
- Upload `Dockerrun.aws.json`
- Configure environment variables

#### Azure App Service
- Create App Service plan
- Deploy Docker container
- Configure connection strings

#### Google Cloud Run
- Build container
- Deploy with Cloud Run
- Configure traffic splitting

---

## 📊 Monitoring & Logging

### Application Monitoring

```bash
# Install and configure Prometheus
sudo apt install -y prometheus

# Configure metrics endpoint
dotnet add package prometheus-net
```

### Structured Logging

```csharp
// Configure Serilog in Program.cs
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/urbapf-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var app = builder.Build();
    app.Logger = Log.Logger;
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
```

### Health Checks

```csharp
// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddCheck("ExternalApi", new ExternalApiHealthCheck());

// Health check endpoint
app.MapHealthChecks("/health");
```

---

## 🧪 Testing Strategy

### Unit Testing (Backend)

```bash
# Run unit tests
dotnet test --logger trx;LogFileName=TestResults.trx
```

### Integration Testing

```bash
# Run integration tests
dotnet test --filter Integration
```

### E2E Testing (Frontend)

```bash
# Run E2E tests
npm run test:e2e
```

### Test Coverage

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### Test Data Management

- Use test containers for database testing
- Mock external services
- Test data factories for consistent test data

---

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0'
      - name: Install dependencies
        run: dotnet restore
      - name: Run tests
        run: dotnet test --logger trx;LogFileName=TestResults.trx

  build-and-deploy:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v3
      - name: Build and push Docker image
        uses: docker/build-push-action@v3
        with:
          context: .
          push: true
          tags: your-registry/urbapf:latest
      - name: Deploy to production
        run: |
          ssh ${{ secrets.SSH_USER }}@${{ secrets.SSH_HOST }} "docker compose -f docker-compose.prod.yml up -d --build"
```

### Pre-commit Hooks

```json
// package.json
{
  "husky": {
    "hooks": {
      "pre-commit": "npm run lint && npm run typecheck && npm run test:unit"
    }
  }
}
```

---

## ⚡ Performance Optimization

### VPS-Specific Optimizations

#### Database Optimization
```sql
-- Create indexes for common queries
CREATE INDEX idx_incidents_location ON incidents USING GIST (location);
CREATE INDEX idx_users_credibility ON users (credibility_level);
```

#### Caching Strategies
```csharp
// Configure distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Configuration.GetConnectionString("Redis");
});

// Cache popular data
[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
public async Task<IActionResult> GetPopularPosts()
{
    return Ok(await _postService.GetPopularPosts());
}
```

#### Image Optimization
- Use WebP format for images
- Implement lazy loading
- Set appropriate image sizes for different devices

#### API Optimization
- Implement pagination for large datasets
- Use HTTP/2 for better performance
- Enable response compression

---

## 🛠️ Troubleshooting Guide

### Common Issues

#### Database Connection Issues
```bash
# Check PostgreSQL status
sudo systemctl status postgresql

# Check connection
psql -h localhost -U postgres -d urbapf

# Common solutions
# 1. Restart PostgreSQL
sudo systemctl restart postgresql

# 2. Check firewall
sudo ufw allow 5432

# 3. Verify credentials in .env
```

#### Docker Issues
```bash
# Check Docker status
sudo systemctl status docker

# Common solutions
# 1. Restart Docker
sudo systemctl restart docker

# 2. Check disk space
df -h

# 3. Clear unused containers
docker system prune -a
```

#### Frontend Build Issues
```bash
# Clear npm cache
npm cache clean --force

# Reinstall dependencies
rm -rf node_modules package-lock.json
npm install

# Common solutions
# 1. Check Node.js version
node --version

# 2. Check Quasar version
quasar --version
```

### Performance Issues

#### Slow API Responses
- Check database query performance
- Verify caching is working
- Monitor server resources

#### High Memory Usage
- Check for memory leaks
- Optimize image sizes
- Implement proper garbage collection

---

## 🔒 Security Best Practices

### Authentication & Authorization
- Use JWT with refresh tokens
- Implement role-based access control (RBAC)
- Secure API endpoints with proper validation

### Data Protection
- Encrypt sensitive data at rest
- Use HTTPS everywhere
- Implement proper CORS policies

### Input Validation
- Validate all user inputs
- Use parameterized queries
- Implement rate limiting

### Security Headers
```csharp
// Configure security headers
app.UseHttpsRedirection();
app.UseHsts();
app.UseCORS(options => options.WithOrigins("https://yourdomain.com"));
```

---

## 📚 API Documentation

### OpenAPI/Swagger Setup

```csharp
// Configure Swagger in Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable Swagger endpoint
app.UseSwagger();
app.UseSwaggerUI();
```

### API Versioning

```csharp
// Configure API versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});
```

---

## 📱 Mobile App Considerations

### Capacitor Configuration

```json
// capacitor.config.json
{
  "appId": "com.urbapf.app",
  "appName": "UrbaPF",
  "webDir": "dist",
  "plugins": {
    "SplashScreen": {
      "launchAutoHide": true,
      "launchShowDuration": 3000
    }
  }
}
```

### Push Notifications

- Configure Firebase Cloud Messaging
- Implement background notification handling
- Add notification permissions

---

## 📦 Backup & Recovery

### Database Backup

```bash
# Automated backup script
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
pg_dump -h localhost -U postgres -d urbapf > /backups/urbapf_$DATE.sql

# Keep last 7 days of backups
find /backups -name "urbapf_*.sql" -mtime +7 -delete
```

### File Storage Backup

- Configure MinIO lifecycle policies
- Implement S3 cross-region replication
- Regular backup verification

---

## 🎯 Development Workflow

### Git Workflow

1. **Feature Branches**: `feature/feature-name`
2. **Code Review**: Required for all changes
3. **Automated Testing**: Run on all pull requests
4. **Deployment**: Manual approval for production

### Code Review Guidelines

- Check for security vulnerabilities
- Verify performance implications
- Ensure code follows project standards
- Test all new functionality

---

## 📋 Project Management

### Issue Tracking

- Use GitHub Issues for bug tracking
- Implement project boards for organization
- Set up milestones for releases

### Documentation

- Keep API documentation up to date
- Document deployment procedures
- Maintain troubleshooting guides

---

## 🚀 Getting Started Checklist

- [ ] Install prerequisites
- [ ] Configure environment variables
- [ ] Set up database
- [ ] Run migrations
- [ ] Start backend services
- [ ] Start frontend development server
- [ ] Test basic functionality
- [ ] Configure production settings
- [ ] Set up monitoring and logging
- [ ] Implement backup procedures
</task_progress>
</write_to_file>