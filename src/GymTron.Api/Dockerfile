# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0.301 AS build
WORKDIR /src

# Copy solution-level build files for consistent compilation and SDK pinning
COPY ["Directory.Build.props", "./"]
COPY ["global.json", "./"]
COPY [".editorconfig", "./"]

# Copy project files for efficient Docker layer caching on restore
COPY ["src/GymTron.Domain/GymTron.Domain.csproj", "src/GymTron.Domain/"]
COPY ["src/GymTron.Application/GymTron.Application.csproj", "src/GymTron.Application/"]
COPY ["src/GymTron.Infrastructure/GymTron.Infrastructure.csproj", "src/GymTron.Infrastructure/"]
COPY ["src/GymTron.Api/GymTron.Api.csproj", "src/GymTron.Api/"]

# Restore dependencies
RUN dotnet restore "src/GymTron.Api/GymTron.Api.csproj"

# Copy source code
COPY ["src/GymTron.Domain/", "src/GymTron.Domain/"]
COPY ["src/GymTron.Application/", "src/GymTron.Application/"]
COPY ["src/GymTron.Infrastructure/", "src/GymTron.Infrastructure/"]
COPY ["src/GymTron.Api/", "src/GymTron.Api/"]

# Publish application
WORKDIR "/src/src/GymTron.Api"
RUN dotnet publish "GymTron.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "GymTron.Api.dll"]
