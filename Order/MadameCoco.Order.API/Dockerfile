# 1. Aşama: Build (Derleme)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala
COPY ["Order/MadameCoco.Order.API/MadameCoco.Order.API.csproj", "Order/MadameCoco.Order.API/"]
COPY ["Shared/MadameCoco.Shared/MadameCoco.Shared.csproj", "Shared/MadameCoco.Shared/"]

# Tüm bağımlılıkları restore et (Shared dahil)
RUN dotnet restore "Order/MadameCoco.Order.API/MadameCoco.Order.API.csproj" --verbosity normal

# Şimdi sadece Order.API ve Shared kaynak kodlarını kopyala
COPY ["Order/MadameCoco.Order.API/", "Order/MadameCoco.Order.API/"]
COPY ["Shared/MadameCoco.Shared/", "Shared/MadameCoco.Shared/"]

# Kaynak kodlar kopyalandıktan sonra restore'u tekrar yap
RUN dotnet restore "Order/MadameCoco.Order.API/MadameCoco.Order.API.csproj"

# Order.API'yi yayımla (Publish)
WORKDIR /src/Order/MadameCoco.Order.API
RUN dotnet publish "MadameCoco.Order.API.csproj" -c Release -o /app/publish --no-restore

# 2. Aşama: Final (Çalışma Ortamı)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Health check için curl yükle
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Health Check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "MadameCoco.Order.API.dll"]
