# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Frontend build
FROM node:20-alpine AS frontend
WORKDIR /frontend
COPY new-dashboard/package*.json ./
RUN npm ci
COPY new-dashboard/ ./
RUN npm run build

# Backend build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend_net/backend_net.csproj", "backend_net/"]
RUN dotnet restore "backend_net/backend_net.csproj"
# Install EF Core tools for migrations
RUN dotnet tool install --global dotnet-ef || true
ENV PATH="${PATH}:/root/.dotnet/tools"
COPY backend_net/ backend_net/
WORKDIR "/src/backend_net"
RUN dotnet build "backend_net.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "backend_net.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime (SDK to allow EF Core migrations)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=frontend /frontend/dist ./wwwroot
RUN dotnet tool install --global dotnet-ef || true
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "backend_net.dll"]