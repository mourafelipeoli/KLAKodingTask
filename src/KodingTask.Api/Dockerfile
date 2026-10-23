# Build context: the repository root.
#   docker build -f src/KodingTask.Api/Dockerfile -t kodingtask .
#   docker run --rm -p 8080:8080 kodingtask          # SQLite inside the container

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# The publish builds the React client through the .esproj reference, so the build stage
# needs Node.js next to the .NET SDK.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates gnupg \
    && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY global.json Directory.Build.props Directory.Build.targets versions.json ./
COPY src/KodingTask.Conversion/KodingTask.Conversion.csproj src/KodingTask.Conversion/
COPY src/KodingTask.Data/KodingTask.Data.csproj src/KodingTask.Data/
COPY src/KodingTask.Api/KodingTask.Api.csproj src/KodingTask.Api/
COPY src/kodingtask-web/kodingtask-web.esproj src/kodingtask-web/package.json src/kodingtask-web/package-lock.json src/kodingtask-web/
RUN dotnet restore src/KodingTask.Api/KodingTask.Api.csproj \
    && cd src/kodingtask-web && npm ci --no-audit --no-fund

COPY . .
RUN dotnet publish src/KodingTask.Api/KodingTask.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
# The default SQLite location must be writable by the non-root user the image runs as.
RUN mkdir -p /app/App_Data && chown app:app /app/App_Data
COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "KodingTask.Api.dll"]
