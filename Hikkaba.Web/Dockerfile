# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
RUN mkdir -p /var/hikkaba/storage \
    && chown -R 1654:1654 /var/hikkaba/storage \
    && apt-get update \
    && apt-get install -y libgdiplus
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Hikkaba.Web/Hikkaba.Web.csproj", "Hikkaba.Web/"]
COPY ["BBCodeParser/BBCodeParser.csproj", "BBCodeParser/"]
COPY ["Hikkaba.Data/Hikkaba.Data.csproj", "Hikkaba.Data/"]
COPY ["Hikkaba.Shared/Hikkaba.Shared.csproj", "Hikkaba.Shared/"]
COPY ["Hikkaba.Infrastructure.Models/Hikkaba.Infrastructure.Models.csproj", "Hikkaba.Infrastructure.Models/"]
COPY ["Hikkaba.Paging/Hikkaba.Paging.csproj", "Hikkaba.Paging/"]
COPY ["Hikkaba.Infrastructure.Repositories/Hikkaba.Infrastructure.Repositories.csproj", "Hikkaba.Infrastructure.Repositories/"]
COPY ["Hikkaba.Infrastructure.Mappings/Hikkaba.Infrastructure.Mappings.csproj", "Hikkaba.Infrastructure.Mappings/"]
COPY ["Hikkaba.Application/Hikkaba.Application.csproj", "Hikkaba.Application/"]
RUN dotnet restore "./Hikkaba.Web/Hikkaba.Web.csproj"
COPY . .
WORKDIR "/src/Hikkaba.Web"
RUN dotnet build "./Hikkaba.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Hikkaba.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hikkaba.Web.dll"]
