# 1. Aşama: Build (Derleme)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala
COPY ["ApiGateways/MadameCoco.Gateway/MadameCoco.Gateway.csproj", "ApiGateways/MadameCoco.Gateway/"]

# Tüm bağımlılıkları restore et
RUN dotnet restore "ApiGateways/MadameCoco.Gateway/MadameCoco.Gateway.csproj" --verbosity normal

# Şimdi sadece Gateway kaynak kodlarını kopyala
COPY ["ApiGateways/MadameCoco.Gateway/", "ApiGateways/MadameCoco.Gateway/"]

# Kaynak kodlar kopyalandıktan sonra restore'u tekrar yap
RUN dotnet restore "ApiGateways/MadameCoco.Gateway/MadameCoco.Gateway.csproj"

# Gateway'i yayımla (Publish)
WORKDIR /src/ApiGateways/MadameCoco.Gateway
RUN dotnet publish "MadameCoco.Gateway.csproj" -c Release -o /app/publish --no-restore

# 2. Aşama: Final (Çalışma Ortamı)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Health check için curl yükle
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Health Check - Gateway'in çalışıp çalışmadığını kontrol eder
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "MadameCoco.Gateway.dll"]

