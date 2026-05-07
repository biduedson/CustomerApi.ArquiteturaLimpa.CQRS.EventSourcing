# -----------------------------------------------------
# 🔹 Configuração da Imagem Base 🔹
# -----------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_NOLOGO=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# -----------------------------------------------------
# 🔹 Configuração da Build 🔹
# -----------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build
WORKDIR /src

# Copia arquivos globais
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["nuget.config", "."]

# Copia os projetos da sua solução
COPY ["src/CustomerApi.WebApi/CustomerApi.WebApi.csproj", "src/CustomerApi.WebApi/"]
COPY ["src/CustomerApi.Application/CustomerApi.Application.csproj", "src/CustomerApi.Application/"]
COPY ["src/CustomerApi.Domain/CustomerApi.Domain.csproj", "src/CustomerApi.Domain/"]
COPY ["src/CustomerApi.Core/CustomerApi.Core.csproj", "src/CustomerApi.Core/"]
COPY ["src/CustomerApi.Infrastructure/CustomerApi.Infrastructure.csproj", "src/CustomerApi.Infrastructure/"]
COPY ["src/CustomerApi.Query/CustomerApi.Query.csproj", "src/CustomerApi.Query/"]

# Restaura dependências
RUN dotnet restore "src/CustomerApi.WebApi/CustomerApi.WebApi.csproj"

# Copia todo o código
COPY . .

# Define diretório para build
WORKDIR "/src/src/CustomerApi.WebApi"
RUN dotnet build "CustomerApi.WebApi.csproj" --configuration Release --no-restore --nologo --output /app/build

# -----------------------------------------------------
# 🔹 Publicação 🔹
# -----------------------------------------------------
FROM build AS publish
RUN dotnet publish "CustomerApi.WebApi.csproj" --configuration Release --no-restore --nologo --output /app/publish

# -----------------------------------------------------
# 🔹 Final 🔹
# -----------------------------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerApi.WebApi.dll"]
