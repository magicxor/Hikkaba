#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
RUN apt-get update \
    && apt-get install -y libgdiplus \
    && mkdir -p /home/hikkaba/keys \
    && chmod -R 777 /home/hikkaba/keys
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Hikkaba.Web/Hikkaba.Web.csproj", "Hikkaba.Web/"]
COPY ["Hikkaba.Data/Hikkaba.Data.csproj", "Hikkaba.Data/"]
COPY ["Hikkaba.Common/Hikkaba.Common.csproj", "Hikkaba.Common/"]
COPY ["Hikkaba.Models/Hikkaba.Models.csproj", "Hikkaba.Models/"]
COPY ["CodeKicker.BBCode/CodeKicker.BBCode.csproj", "CodeKicker.BBCode/"]
COPY ["Hikkaba.Services/Hikkaba.Services.csproj", "Hikkaba.Services/"]
COPY ["Hikkaba.Infrastructure/Hikkaba.Infrastructure.csproj", "Hikkaba.Infrastructure/"]
RUN dotnet restore "Hikkaba.Web/Hikkaba.Web.csproj"
COPY . .
WORKDIR "/src/Hikkaba.Web"
RUN dotnet build "Hikkaba.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hikkaba.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hikkaba.Web.dll"]