# Build stage - .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copiar la solución y los proyectos manteniendo la estructura de carpetas
# Se asume que el Dockerfile está en la raíz del repositorio
COPY backend/UrbaPF.sln ./
COPY backend/Api/UrbaPF.Api.csproj ./backend/Api/
COPY backend/Domain/UrbaPF.Domain.csproj ./backend/Domain/
COPY backend/Infrastructure/UrbaPF.Infrastructure.csproj ./backend/Infrastructure/
COPY backend/tests/UrbaPF.Tests.csproj ./backend/tests/

# 2. Restaurar dependencias
RUN dotnet restore

# 3. Copiar el resto del código fuente
COPY . .

# 4. Publicar la aplicación
WORKDIR "/src/backend/Api"
RUN dotnet publish "UrbaPF.Api.csproj" -c Release -o /app/publish

# Final stage - .NET 10 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Instalar utilitarios necesarios para salud y diagnóstico
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    procps \
    && rm -rf /var/lib/apt/lists/*

# Configuración de seguridad: Usuario no-root
# Nota: dotnet 8+ ya trae un usuario 'app' por defecto, pero creamos uno personalizado por consistencia
RUN groupadd -r appgroup && useradd -r -g appgroup appuser
COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app

# Cambiar a usuario no-root
USER appuser

# Configuración de entorno
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Ejecución
ENTRYPOINT ["dotnet", "UrbaPF.Api.dll"]