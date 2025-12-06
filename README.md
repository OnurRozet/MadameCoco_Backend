# MadameCoco Backend - Microservices Architecture

Modern, ölçeklenebilir ve yüksek performanslı bir e-ticaret backend sistemi. Microservices mimarisi kullanılarak geliştirilmiş, .NET 9.0 tabanlı bir projedir.

## 📋 İçindekiler

- [Genel Bakış](#genel-bakış)
- [Mimari](#mimari)
- [Teknolojiler](#teknolojiler)
- [Servisler](#servisler)
- [Kurulum](#kurulum)
- [Çalıştırma](#çalıştırma)
- [API Endpointleri](#api-endpointleri)
- [Health Check](#health-check)
- [Test](#test)
- [Docker](#docker)
- [CI/CD](#cicd)
- [Katkıda Bulunma](#katkıda-bulunma)

## 🎯 Genel Bakış

MadameCoco Backend, müşteri yönetimi, sipariş işleme ve audit loglama gibi temel e-ticaret işlevlerini sağlayan bir microservices ekosistemidir. Her servis bağımsız olarak geliştirilebilir, test edilebilir ve dağıtılabilir.

### Özellikler

- ✅ **Microservices Mimarisi** - Bağımsız ve ölçeklenebilir servisler
- ✅ **API Gateway** - Ocelot ile merkezi yönlendirme
- ✅ **Event-Driven Architecture** - RabbitMQ ile asenkron iletişim
- ✅ **CQRS Pattern** - MediatR ile komut/sorgu ayrımı
- ✅ **Health Checks** - Servis sağlık durumu izleme
- ✅ **Docker Support** - Containerization desteği
- ✅ **CI/CD Pipeline** - GitHub Actions ile otomatik deployment
- ✅ **Comprehensive Testing** - Unit ve Integration testler

## 🏗️ Mimari

```
┌─────────────────────────────────────────────────────────────┐
│                      API Gateway (Ocelot)                    │
│                      Port: 5000/5001                        │
└──────────────┬──────────────────────────────┬────────────────┘
               │                              │
       ┌───────▼───────┐            ┌─────────▼────────┐
       │ Customer API  │            │   Order API      │
       │  Port: 7000   │            │   Port: 7001     │
       │               │            │                  │
       │  SQL Server   │            │   SQL Server     │
       └───────┬───────┘            └─────────┬────────┘
               │                              │
               │         ┌─────────────────────┘
               │         │
       ┌───────▼─────────▼────────┐
       │    RabbitMQ (MassTransit) │
       │    Port: 5672/15672       │
       └───────┬───────────────────┘
               │
       ┌───────▼──────────┐
       │  Audit Worker    │
       │  Port: 8080      │
       │                  │
       │  MongoDB         │
       │  Redis (Hangfire)│
       └──────────────────┘
```

## 🛠️ Teknolojiler

### Backend Framework
- **.NET 9.0** - En son .NET sürümü
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 9.0** - ORM

### Mimari Desenler & Kütüphaneler
- **MediatR** - CQRS pattern implementasyonu
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **MassTransit** - Message bus (RabbitMQ)
- **Ocelot** - API Gateway

### Veritabanları
- **SQL Server 2022** - İlişkisel veritabanı (Customer & Order)
- **MongoDB** - NoSQL veritabanı (Audit logs)
- **Redis** - Cache ve Hangfire için

### Mesajlaşma & Kuyruk
- **RabbitMQ** - Message broker
- **Hangfire** - Background job processing

### Diğer
- **Swagger/OpenAPI** - API dokümantasyonu
- **Health Checks** - Servis sağlık izleme
- **Docker** - Containerization
- **GitHub Actions** - CI/CD

## 🚀 Servisler

### 1. Customer API
Müşteri yönetimi servisi. Müşteri CRUD işlemlerini yönetir.

- **Port:** `7000` (HTTP), `5274/5275` (HTTPS)
- **Veritabanı:** SQL Server
- **Endpoint Base:** `/api/customers`
- **Pattern:** Repository Design Pattern

### 2. Order API
Sipariş yönetimi servisi. Sipariş oluşturma ve sorgulama işlemlerini yönetir.

- **Port:** `7001` (HTTP), `5036/5037` (HTTPS)
- **Veritabanı:** SQL Server
- **Endpoint Base:** `/api/orders`
- **Pattern:** CQRS (MediatR)

### 3. Audit Worker
Audit loglama ve e-posta bildirim servisi. RabbitMQ üzerinden event dinler.

- **Port:** `8080`
- **Veritabanı:** MongoDB (logs), Redis (Hangfire)
- **Özellikler:**
  - Order event'lerini dinler
  - MongoDB'ye log kaydeder
  - E-posta bildirimleri gönderir
  - Hangfire dashboard (`/hangfire`)

### 4. API Gateway
Tüm servislere tek bir giriş noktası sağlar.

- **Port:** `5000` (HTTP), `5001/7115` (HTTPS)
- **Framework:** Ocelot
- **Routing:** 
  - `/customers/*` → Customer API
  - `/orders/*` → Order API

## 📦 Kurulum

### Gereksinimler

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (veya Docker + Docker Compose)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (veya Docker container)
- [Git](https://git-scm.com/downloads)

### Adımlar

1. **Repository'yi klonlayın:**
```bash
git clone <repository-url>
cd MadameCoco_Backend
```

2. **Docker container'ları başlatın:**
```bash
docker-compose up -d
```

Bu komut şu servisleri başlatır:
- SQL Server (Port: 1433)
- MongoDB (Port: 27017)
- RabbitMQ (Port: 5672, Management UI: 15672)
- Redis (Port: 6379)

3. **Veritabanı migration'larını çalıştırın:**

Customer API için:
```bash
cd Customer/MadameCoco.Customer.API
dotnet ef database update
```

Order API için:
```bash
cd Order/MadameCoco.Order.API
dotnet ef database update
```

4. **NuGet paketlerini restore edin:**
```bash
dotnet restore
```

## ▶️ Çalıştırma

### Tüm Servisleri Çalıştırma

**Visual Studio:**
- Solution'ı açın (`MadameCoco_Backend.sln`)
- Multiple startup projects olarak ayarlayın:
  - `MadameCoco.Gateway`
  - `MadameCoco.Customer.API`
  - `MadameCoco.Order.API`
  - `MadameCoco.Audit.Worker`

**Command Line:**
```bash
# Terminal 1 - Gateway
cd ApiGateways/MadameCoco.Gateway
dotnet run

# Terminal 2 - Customer API
cd Customer/MadameCoco.Customer.API
dotnet run

# Terminal 3 - Order API
cd Order/MadameCoco.Order.API
dotnet run

# Terminal 4 - Audit Worker
cd Audit/MadameCoco.Audit.Worker
dotnet run
```

### Docker ile Çalıştırma

```bash
# Servisleri build et
docker-compose -f docker-compose.yml build

# Servisleri başlat
docker-compose -f docker-compose.yml up -d
```

## 📡 API Endpointleri

### Gateway Üzerinden (Önerilen)

**Base URL:** `http://localhost:5001`

#### Customer Endpoints
```
GET    /customers              # Tüm müşterileri listele
GET    /customers/{id}         # Müşteri detayı
GET    /customers/validate/{id} # Müşteri doğrulama
POST   /customers              # Yeni müşteri oluştur
PUT    /customers              # Müşteri güncelle
DELETE /customers/{id}         # Müşteri sil
```

#### Order Endpoints
```
GET    /orders                 # Tüm siparişleri listele
GET    /orders/{id}            # Sipariş detayı
POST   /orders                 # Yeni sipariş oluştur
```

### Doğrudan Servisler

**Customer API:** `http://localhost:7000/api/customers`
**Order API:** `http://localhost:7001/api/orders`

### Swagger UI

Development ortamında Swagger UI erişilebilir:

- Gateway: `http://localhost:5001/swagger`
- Customer API: `http://localhost:7000/swagger`
- Order API: `http://localhost:7001/swagger`

## 🏥 Health Check

Tüm servisler health check endpoint'leri sağlar:

### Endpoint'ler

- `/health` - Genel sağlık durumu (tüm kontroller)
- `/health/ready` - Servis hazır mı? (database bağlantısı dahil)
- `/health/live` - Servis çalışıyor mu? (basit kontrol)

### Örnek Kullanım

**PowerShell:**
```powershell
# Customer API health check
Invoke-WebRequest -Uri "http://localhost:7000/health" -Method GET

# Order API ready check
Invoke-WebRequest -Uri "http://localhost:7001/health/ready" -Method GET
```

**cURL:**
```bash
curl http://localhost:7000/health
curl http://localhost:7001/health/ready
curl http://localhost:5000/health/live
```

**Tarayıcı:**
```
http://localhost:7000/health
http://localhost:7001/health/ready
http://localhost:5000/health
```

## 🧪 Test

### Unit Testler

```bash
# Customer API testleri
dotnet test Customer/MadameCoco.Customer.Tests/MadameCoco.Customer.Tests.csproj

# Order API testleri
dotnet test Order/MadameCoco.Order.Tests/MadameCoco.Order.Tests.csproj
```

### Integration Testler

Integration testler otomatik olarak çalışır. InMemory database kullanır.

### Test Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 🐳 Docker

### Dockerfile'lar

Her servis için ayrı Dockerfile mevcuttur:

- `Customer/MadameCoco.Customer.API/Dockerfile`
- `Order/MadameCoco.Order.API/Dockerfile`
- `Audit/MadameCoco.Audit.Worker/Dockerfile`
- `ApiGateways/MadameCoco.Gateway/Dockerfile`

### Docker Image Build

```bash
# Customer API
docker build -f Customer/MadameCoco.Customer.API/Dockerfile -t madamecoco-customer-api:latest .

# Order API
docker build -f Order/MadameCoco.Order.API/Dockerfile -t madamecoco-order-api:latest .

# Audit Worker
docker build -f Audit/MadameCoco.Audit.Worker/Dockerfile -t madamecoco-audit-worker:latest .

# Gateway
docker build -f ApiGateways/MadameCoco.Gateway/Dockerfile -t madamecoco-gateway:latest .
```

### Docker Hub

CI/CD pipeline otomatik olarak Docker Hub'a push yapar:
- `{DOCKER_USERNAME}/madamecoco-customer-api:latest`
- `{DOCKER_USERNAME}/madamecoco-order-api:latest`
- `{DOCKER_USERNAME}/madamecoco-audit-worker:latest`
- `{DOCKER_USERNAME}/madamecoco-gateway:latest`

## 🔄 CI/CD

GitHub Actions ile otomatik CI/CD pipeline yapılandırılmıştır.

### Pipeline Adımları

1. **Test Aşaması**
   - Unit testler çalıştırılır
   - Integration testler çalıştırılır
   - Test sonuçları yayınlanır

2. **Build & Push Aşaması**
   - Docker image'ları build edilir
   - Docker Hub'a push edilir

3. **Deployment Aşaması** (Opsiyonel)
   - SSH ile sunucuya bağlanır
   - Yeni image'lar çekilir
   - Servisler yeniden başlatılır

### GitHub Secrets Gereksinimleri

Deployment için şu secrets'lar yapılandırılmalıdır:

- `DOCKER_USERNAME` - Docker Hub kullanıcı adı
- `DOCKER_PASSWORD` - Docker Hub şifresi
- `SSH_HOST` - Sunucu IP/domain
- `SSH_USERNAME` - SSH kullanıcı adı
- `SSH_PRIVATE_KEY` - SSH private key
- `SSH_PORT` - SSH port (varsayılan: 22)

### Pipeline Dosyası

`.github/workflows/deploy.yml`

## 🔧 Yapılandırma

### Connection Strings

**Customer API** (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MadameCoco_CustomerDb;User Id=sa;Password=MadameCoco_2024!;TrustServerCertificate=True;"
  }
}
```

**Order API** (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MadameCoco_OrderDb;User Id=sa;Password=MadameCoco_2024!;TrustServerCertificate=True;"
  }
}
```

### RabbitMQ Yapılandırması

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "User": "guest",
    "Pass": "guest"
  }
}
```

### MongoDB Yapılandırması

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "MadameCoco_AuditDb"
  }
}
```

## 📊 Monitoring & Logging

### Hangfire Dashboard

Audit Worker'da periyodik görevleri izlemek için:

```
http://localhost:8080/hangfire
```

### RabbitMQ Management UI

RabbitMQ queue'larını izlemek için:

```
http://localhost:15672
```

Kullanıcı adı: `guest`
Şifre: `guest`

### Hangfire E-posta Raporları

Audit Worker, Hangfire ile periyodik olarak (Hergün saat 10:00 da ancak test amaçlı varsayılan 10 dk da bir) yöneticilere audit raporu e-postası gönderir. Bu rapor, son periyotta gerçekleşen sipariş olaylarını ve tahmini toplam ciroyu içerir.


#### Rapor Özellikleri

- ✅ **Periyodik Gönderim** - Varsayılan olarak her 10 dakikada bir
- ✅ **Sipariş Özeti** - Son periyottaki toplam sipariş sayısı
- ✅ **Ciro Bilgisi** - Tahmini toplam ciro
- ✅ **Detaylı Liste** - Sipariş ID, Müşteri ID, Ürün bilgileri, Tutar ve Zaman
- ✅ **Otomatik Oluşturma** - Hangfire tarafından otomatik olarak oluşturulur

#### E-posta Yapılandırması

E-posta ayarları `appsettings.json` dosyasında yapılandırılabilir:

```json
{
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "FromEmail": "noreply@madamecoco.com",
    "FromName": "Madame Coco Sistem",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "ToEmail": "admin@madamecoco.com"
  }
}
```

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'Add some amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📝 Lisans

Bu proje özel bir projedir.

## 👥 İletişim

Sorularınız için issue açabilirsiniz.

---

**Not:** Bu README dosyası projenin genel bir özetidir. Detaylı bilgi için kod içi yorumları ve Swagger dokümantasyonunu inceleyebilirsiniz.

