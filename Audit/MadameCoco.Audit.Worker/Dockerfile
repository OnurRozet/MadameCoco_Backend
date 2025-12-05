# 1. Aşama: Build (Derleme)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala
COPY ["Audit/MadameCoco.Audit.Worker/MadameCoco.Audit.Worker.csproj", "Audit/MadameCoco.Audit.Worker/"]
COPY ["Shared/MadameCoco.Shared/MadameCoco.Shared.csproj", "Shared/MadameCoco.Shared/"]

# Tüm bağımlılıkları restore et (Shared dahil)
RUN dotnet restore "Audit/MadameCoco.Audit.Worker/MadameCoco.Audit.Worker.csproj" --verbosity normal

# Şimdi sadece Audit.Worker ve Shared kaynak kodlarını kopyala
COPY ["Audit/MadameCoco.Audit.Worker/", "Audit/MadameCoco.Audit.Worker/"]
COPY ["Shared/MadameCoco.Shared/", "Shared/MadameCoco.Shared/"]

# Audit.Worker'yi yayımla (Publish)
WORKDIR /src/Audit/MadameCoco.Audit.Worker
RUN dotnet publish "MadameCoco.Audit.Worker.csproj" -c Release -o /app/publish --no-restore

# 2. Aşama: Final (Çalışma Ortamı)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Health check için curl yükle
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Health Check - MongoDB ve RabbitMQ bağlantılarını kontrol eder
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "MadameCoco.Audit.Worker.dll"]
