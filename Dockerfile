# Multi-stage Dockerfile for C# .NET 9 Web API

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first for efficient caching
COPY CCTV.Analytics/CCTV.Analytics.sln ./CCTV.Analytics/
COPY CCTV.Analytics/src/CCTV.Api/CCTV.Api.csproj ./CCTV.Analytics/src/CCTV.Api/
COPY CCTV.Analytics/src/CCTV.Application/CCTV.Application.csproj ./CCTV.Analytics/src/CCTV.Application/
COPY CCTV.Analytics/src/CCTV.Domain/CCTV.Domain.csproj ./CCTV.Analytics/src/CCTV.Domain/
COPY CCTV.Analytics/src/CCTV.Infrastructure/CCTV.Infrastructure.csproj ./CCTV.Analytics/src/CCTV.Infrastructure/
COPY CCTV.Analytics/src/CCTV.Persistence/CCTV.Persistence.csproj ./CCTV.Analytics/src/CCTV.Persistence/
COPY CCTV.Analytics/src/CCTV.Contracts/CCTV.Contracts.csproj ./CCTV.Analytics/src/CCTV.Contracts/
COPY CCTV.Analytics/src/CCTV.Shared/CCTV.Shared.csproj ./CCTV.Analytics/src/CCTV.Shared/

RUN dotnet restore ./CCTV.Analytics/CCTV.Analytics.sln

# Copy the rest of the source code and build
COPY CCTV.Analytics/ ./CCTV.Analytics/
WORKDIR /src/CCTV.Analytics/src/CCTV.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runner
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CCTV.Api.dll"]
