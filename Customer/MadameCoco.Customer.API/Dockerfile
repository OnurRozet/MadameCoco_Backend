# 1. Aşama: Build (Derleme)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Önce sadece proje dosyalarını kopyala (Docker cache için önemli)
COPY ["Customer/MadameCoco.Customer.API/MadameCoco.Customer.API.csproj", "Customer/MadameCoco.Customer.API/"]
COPY ["Shared/MadameCoco.Shared/MadameCoco.Shared.csproj", "Shared/MadameCoco.Shared/"]

# Tüm bağımlılıkları restore et (Shared dahil)
RUN dotnet restore "Customer/MadameCoco.Customer.API/MadameCoco.Customer.API.csproj" --verbosity normal

# Şimdi sadece Customer.API ve Shared kaynak kodlarını kopyala (diğer projeleri kopyalama)
COPY ["Customer/MadameCoco.Customer.API/", "Customer/MadameCoco.Customer.API/"]
COPY ["Shared/MadameCoco.Shared/", "Shared/MadameCoco.Shared/"]

# Kaynak kodlar kopyalandıktan sonra restore'u tekrar yap (cache sorunlarını önlemek için)
RUN dotnet restore "Customer/MadameCoco.Customer.API/MadameCoco.Customer.API.csproj"

# Customer.API'yi yayımla (Publish)
WORKDIR /src/Customer/MadameCoco.Customer.API
RUN dotnet publish "MadameCoco.Customer.API.csproj" -c Release -o /app/publish --no-restore

# 2. Aşama: Final (Çalışma Ortamı)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Health check için curl yükle (Debian tabanlı image)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Health Check - Docker ve Kubernetes tarafından kullanılır
# Her 30 saniyede bir kontrol eder, 10 saniye timeout, 3 kez başarısız olursa unhealthy
# Port 8080 yerine ASPNETCORE_URLS environment variable'ından port alınır
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "MadameCoco.Customer.API.dll"]
