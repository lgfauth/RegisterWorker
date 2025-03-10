FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Worker/RegisterWorker.csproj", "src/Worker/"]
RUN dotnet restore "./src/Worker/RegisterWorker.csproj"

COPY . .

WORKDIR "/src/src/Worker"
RUN dotnet build "./RegisterWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RegisterWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "RegisterWorker.dll"]