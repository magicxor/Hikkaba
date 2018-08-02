FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1709 AS base
WORKDIR /app
EXPOSE 55712
EXPOSE 44375

FROM microsoft/dotnet:2.1-sdk-nanoserver-1709 AS build
WORKDIR /src
COPY Hikkaba.Web/Hikkaba.Web.csproj Hikkaba.Web/
RUN dotnet restore Hikkaba.Web/Hikkaba.Web.csproj
COPY . .
WORKDIR /src/Hikkaba.Web
RUN dotnet build Hikkaba.Web.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Hikkaba.Web.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Hikkaba.Web.dll"]
