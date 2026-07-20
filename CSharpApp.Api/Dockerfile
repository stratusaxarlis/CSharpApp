FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CSharpApp.Api/CSharpApp.Api.csproj", "CSharpApp.Api/"]
COPY ["CSharpApp.Core/CSharpApp.Core.csproj", "CSharpApp.Core/"]
COPY ["CSharpApp.Infrastructure/CSharpApp.Infrastructure.csproj", "CSharpApp.Infrastructure/"]
COPY ["CSharpApp.Application/CSharpApp.Application.csproj", "CSharpApp.Application/"]
RUN dotnet restore "CSharpApp.Api/CSharpApp.Api.csproj"
COPY . .
WORKDIR "/src/CSharpApp.Api"
RUN dotnet build "./CSharpApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CSharpApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CSharpApp.Api.dll"]
