# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update \
    && apt-get install -y libgdiplus \
    && mkdir -p /home/hikkaba/keys \
    && chmod -R 777 /home/hikkaba/keys
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Hikkaba.Web/Hikkaba.Web.csproj", "Hikkaba.Web/"]
COPY ["Hikkaba.Data/Hikkaba.Data.csproj", "Hikkaba.Data/"]
COPY ["Hikkaba.Common/Hikkaba.Common.csproj", "Hikkaba.Common/"]
COPY ["Hikkaba.Models/Hikkaba.Models.csproj", "Hikkaba.Models/"]
COPY ["CodeKicker.BBCode/CodeKicker.BBCode.csproj", "CodeKicker.BBCode/"]
COPY ["Hikkaba.Services/Hikkaba.Services.csproj", "Hikkaba.Services/"]
COPY ["Hikkaba.Infrastructure/Hikkaba.Infrastructure.csproj", "Hikkaba.Infrastructure/"]
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
