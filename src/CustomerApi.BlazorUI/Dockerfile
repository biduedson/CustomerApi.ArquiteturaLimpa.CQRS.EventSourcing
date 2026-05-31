# =============================================
# 1. Base (Runtime)
# =============================================
# Esta imagem tem apenas o necessario para rodar a aplicacao ASP.NET/Blazor.
# Ela e menor que a imagem SDK, por isso fica para a etapa final.
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS base

# Define a pasta onde a aplicacao vai rodar dentro do container.
WORKDIR /app

# Informa que o container vai escutar na porta 80.
EXPOSE 80

# Faz o ASP.NET Core ouvir requisicoes HTTP na porta 80.
ENV ASPNETCORE_URLS=http://+:80

# Define o ambiente padrao da aplicacao dentro do container.
ENV ASPNETCORE_ENVIRONMENT=Production

# Remove mensagens extras do .NET e desativa telemetria da CLI.
ENV DOTNET_NOLOGO=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# =============================================
# 2. Build
# =============================================
# Esta imagem tem o SDK completo do .NET, usado para restaurar pacotes e compilar.
FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build

# Pasta de trabalho usada durante a compilacao.
WORKDIR /src

# Copia arquivos de configuracao primeiro para aproveitar o cache do Docker.
# Assim, o restore so roda de novo quando dependencias/configuracoes mudarem.
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["nuget.config", "."]
COPY ["CustomerApi.ArquiteturaLimpa.CQRS.EventSourcing.slnx", "./"]

# Copia o arquivo do projeto da UI antes do restante do codigo.
# Isso tambem melhora o cache da etapa de restore.
COPY ["src/CustomerApi.BlazorUI/CustomerApi.BlazorUI.csproj", "src/CustomerApi.BlazorUI/"]

# Baixa/restaura os pacotes NuGet usados pela camada UI.
RUN dotnet restore "src/CustomerApi.BlazorUI/CustomerApi.BlazorUI.csproj"

# Copia todo o restante do codigo-fonte para dentro da imagem de build.
COPY . .

# Entra na pasta do projeto UI e compila em modo Release.
WORKDIR "/src/src/CustomerApi.BlazorUI"
RUN dotnet build "CustomerApi.BlazorUI.csproj" --configuration Release --no-restore

# =============================================
# 3. Publish
# =============================================
# Publica a aplicacao em uma pasta pronta para ser executada no runtime.
FROM build AS publish
RUN dotnet publish "CustomerApi.BlazorUI.csproj" --configuration Release --no-restore --output /app/publish

# =============================================
# 4. Final
# =============================================
# Volta para a imagem leve de runtime.
FROM base AS final

# Define a pasta final da aplicacao.
WORKDIR /app

# Copia somente os arquivos publicados, sem SDK e sem codigo-fonte completo.
COPY --from=publish /app/publish .

# Comando executado quando o container iniciar.
ENTRYPOINT ["dotnet", "CustomerApi.BlazorUI.dll"]
