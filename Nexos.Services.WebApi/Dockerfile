FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Nexos.Services.WebApi/Nexos.Services.WebApi.csproj", "Nexos.Services.WebApi/"]
RUN dotnet restore "Nexos.Services.WebApi/Nexos.Services.WebApi.csproj"
COPY . .
WORKDIR "/src/Nexos.Services.WebApi"
RUN dotnet build "./Nexos.Services.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Nexos.Services.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nexos.Services.WebApi.dll"]
