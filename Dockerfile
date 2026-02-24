# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first (better layer caching)
COPY DreamSoft.Api/DreamSoft.Api.csproj                         DreamSoft.Api/
COPY DreamSoft.Application/DreamSoft.Application.csproj         DreamSoft.Application/
COPY DreamSoft.Domain/DreamSoft.Domain.csproj                   DreamSoft.Domain/
COPY DreamSoft.Infrastructure/DreamSoft.Infrastructure.csproj   DreamSoft.Infrastructure/

# Restore only the API project (pulls all referenced projects automatically)
RUN dotnet restore DreamSoft.Api/DreamSoft.Api.csproj

# Copy the rest of the source code
COPY DreamSoft.Api/           DreamSoft.Api/
COPY DreamSoft.Application/   DreamSoft.Application/
COPY DreamSoft.Domain/        DreamSoft.Domain/
COPY DreamSoft.Infrastructure/ DreamSoft.Infrastructure/

# Build and publish in Release mode
RUN dotnet publish DreamSoft.Api/DreamSoft.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Railway injects PORT dynamically — ASP.NET Core reads it via ASPNETCORE_URLS
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

EXPOSE 8080

ENTRYPOINT ["dotnet", "DreamSoft.Api.dll"]
