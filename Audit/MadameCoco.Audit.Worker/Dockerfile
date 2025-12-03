# 1. Aşama: Build (Derleme)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala
COPY ["src/Services/Audit/MadameCoco.Audit.Worker.csproj", "src/Services/Audit/"]
# Gerekli ise Shared/Common projeleri kopyala
 COPY ["src/Shared/MadameCoco.Shared/MadameCoco.Shared.csproj", "src/Shared/MadameCoco.Shared/"] 

# Gerekli tüm projeleri restore et
RUN dotnet restore "src/Services/Audit/MadameCoco.Audit.Worker.csproj"

# Tüm proje dosyalarını kopyala
COPY . .

# Audit.Worker'yi yayımla (Publish)
WORKDIR /src/src/Services/Audit
RUN dotnet publish "MadameCoco.Audit.Worker.csproj" -c Release -o /app/publish --no-restore

# 2. Aşama: Final (Çalışma Ortamı)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MadameCoco.Audit.Worker.dll"]