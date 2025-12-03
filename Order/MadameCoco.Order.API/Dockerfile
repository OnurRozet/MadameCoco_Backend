# 1. Aşama: Build (Derleme)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala
COPY ["src/Services/Order/MadameCoco.Order.API.csproj", "src/Services/Order/"]
# Gerekli ise Shared/Common projeleri kopyala
 COPY ["src/Shared/MadameCoco.Shared/MadameCoco.Shared.csproj", "src/Shared/MadameCoco.Shared/"] 

# Gerekli tüm projeleri restore et
RUN dotnet restore "src/Services/Order/MadameCoco.Order.API.csproj"

# Tüm proje dosyalarını kopyala
COPY . .

# Order.API'yi yayımla (Publish)
WORKDIR /src/src/Services/Order
RUN dotnet publish "MadameCoco.Order.API.csproj" -c Release -o /app/publish --no-restore

# 2. Aşama: Final (Çalışma Ortamı)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MadameCoco.Order.API.dll"]