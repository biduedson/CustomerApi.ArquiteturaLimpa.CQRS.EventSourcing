# =============================================
# 1. Base (Runtime) - Leve
# =============================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_NOLOGO=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# =============================================
# 2. Build
# =============================================
FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build
WORKDIR /src

# Copia arquivos de solução primeiro (melhor cache)
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["nuget.config", "."]
COPY ["CustomerApi.ArquituralLimpa.CQRS.EventSourcing.sln", "./"]

# Copia os csproj
COPY ["src/CustomerApi.WebApi/CustomerApi.WebApi.csproj", "src/CustomerApi.WebApi/"]
COPY ["src/CustomerApi.Application/CustomerApi.Application.csproj", "src/CustomerApi.Application/"]
COPY ["src/CustomerApi.Domain/CustomerApi.Domain.csproj", "src/CustomerApi.Domain/"]
COPY ["src/CustomerApi.Core/CustomerApi.Core.csproj", "src/CustomerApi.Core/"]
COPY ["src/CustomerApi.Infrastructure/CustomerApi.Infrastructure.csproj", "src/CustomerApi.Infrastructure/"]
COPY ["src/CustomerApi.Query/CustomerApi.Query.csproj", "src/CustomerApi.Query/"]

RUN dotnet restore "src/CustomerApi.WebApi/CustomerApi.WebApi.csproj"

# Copia todo o código fonte
COPY . .

WORKDIR "/src/src/CustomerApi.WebApi"
RUN dotnet build "CustomerApi.WebApi.csproj" --configuration Release --no-restore

# =============================================
# 3. Publish
# =============================================
FROM build AS publish
RUN dotnet publish "CustomerApi.WebApi.csproj" --configuration Release --no-restore --output /app/publish

# =============================================
# 4. Final
# =============================================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CustomerApi.WebApi.dll"]