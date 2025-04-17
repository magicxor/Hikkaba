# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
# Create directory, set ownership, update package lists, and install libgdiplus for graphics support
RUN mkdir -p /var/hikkaba/storage \
    && chown -R 1654:1654 /var/hikkaba/storage \
    && apt-get update \
    && apt-get install -y --no-install-recommends libgdiplus \
    && rm -rf /var/lib/apt/lists/*
# Switch back to the non-root application user
USER $APP_UID
WORKDIR /app
# Expose ports for the application
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
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
# Restore dependencies for the main web project
RUN dotnet restore "./Hikkaba.Web/Hikkaba.Web.csproj"
# Copy the rest of the source code
COPY . .
WORKDIR "/src/Hikkaba.Web"
ARG BUILD_CONFIGURATION=Release
ARG ASSEMBLY_VERSION
ARG PRODUCT_VERSION
# Build the project, conditionally adding version parameters
RUN \
  BUILD_CMD="dotnet build \"./Hikkaba.Web.csproj\" -c $BUILD_CONFIGURATION -o /app/build"; \
  if [ -n "$ASSEMBLY_VERSION" ]; then \
    BUILD_CMD="$BUILD_CMD /p:AssemblyVersion=$ASSEMBLY_VERSION"; \
  fi; \
  if [ -n "$PRODUCT_VERSION" ]; then \
    BUILD_CMD="$BUILD_CMD /p:Version=$PRODUCT_VERSION"; \
  fi; \
  # Execute the constructed build command
  eval $BUILD_CMD


# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Declare build arguments for versioning again for this stage
ARG ASSEMBLY_VERSION
ARG PRODUCT_VERSION
WORKDIR "/src/Hikkaba.Web"
# Publish the project, conditionally adding version parameters
RUN \
  PUBLISH_CMD="dotnet publish \"./Hikkaba.Web.csproj\" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false"; \
  if [ -n "$ASSEMBLY_VERSION" ]; then \
    PUBLISH_CMD="$PUBLISH_CMD /p:AssemblyVersion=$ASSEMBLY_VERSION"; \
  fi; \
  if [ -n "$PRODUCT_VERSION" ]; then \
    PUBLISH_CMD="$PUBLISH_CMD /p:Version=$PRODUCT_VERSION"; \
  fi; \
  # Execute the constructed publish command
  eval $PUBLISH_CMD


# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
# Copy the published output from the publish stage
COPY --from=publish /app/publish .
# Set the entry point for the container
ENTRYPOINT ["dotnet", "Hikkaba.Web.dll"]
