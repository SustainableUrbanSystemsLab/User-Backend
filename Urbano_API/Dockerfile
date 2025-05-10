# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy project files first for better layer caching
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App

# Copy build output and assets (if needed)
COPY --from=build-env /App/out .
# Only if Assets are required at runtime
COPY --from=build-env /App/Assets ./Assets/  

# Runtime configuration
EXPOSE 5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

# Health check (recommended for Render)
HEALTHCHECK --interval=30s --timeout=3s \
  CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "Urbano_API.dll"]