# .NET 8 Enterprise Microservices Boilerplate

Üretime hazır (production-ready), **Clean Architecture** ve **Domain-Driven Design (DDD)** ilkeleriyle inşa edilmiş, .NET 8 tabanlı Enterprise Microservices Boilerplate.

---

## Mimari Genel Bakış

Sistem **Onion Architecture** (Soğan Mimarisi) üzerine kurulmuştur. Katmanlar arası bağımlılık kuralı kesindir: dış katmanlar (Infrastructure, Presentation) iç katmanlara (Domain, Application) bağımlıdır; tersi asla geçerli değildir.

```
┌───────────────────────────────────────────────────┐
│                 Presentation Layer                │
│         (API Controllers, Middleware)             │
├───────────────────────────────────────────────────┤
│               Infrastructure Layer                │
│    (EF Core, Repositories, External Services)     │
├───────────────────────────────────────────────────┤
│                Application Layer                  │
│    (CQRS Handlers, DTOs, Specifications, Mappers) │
├───────────────────────────────────────────────────┤
│                  Domain Layer                     │
│  (Entities, Value Objects, Domain Events, Enums)  │
└───────────────────────────────────────────────────┘
```

### Katman Sorumlulukları

| Katman | Sorumluluk | Bağımlılık |
|--------|-----------|------------|
| **Domain** | Entity, Value Object, Aggregate Root, Domain Event, Enum tanımları. İş kurallarının kalbi. | Sıfır dış bağımlılık (yalnızca MediatR.Contracts) |
| **Application** | CQRS Command/Query Handler, DTO, Mapper, Validator, Specification, Interface tanımları. | Yalnızca Domain |
| **Infrastructure** | EF Core DbContext, Repository implementasyonları, JWT Provider, Vault entegrasyonu. | Domain + Application |
| **Presentation** | API Controller, Middleware (TraceId, Idempotency, GlobalException), ApiResponse zarfı. | Tüm katmanlar |

---

## Mikroservisler

### 1. Gateway.Api (API Gateway)
YARP (Yet Another Reverse Proxy) tabanlı merkezi API Gateway. Tüm istemci trafiği bu noktadan girer.

**Sorumlulukları:**
- JWT token doğrulama ve yetkilendirme
- Rol tabanlı politikalar (`Authenticated`, `AdminOnly`, `EmployeeOrAdmin`)
- YARP ile downstream servislere reverse proxy
- HealthChecks UI Dashboard (`/health-ui`)

### 2. AuthService
Kimlik doğrulama ve yetkilendirme servisi.

**Sorumlulukları:**
- JWT Access Token + Refresh Token üretimi
- BCrypt ile güvenli parola doğrulama
- Rol tabanlı erişim yönetimi (User, Employee, Manager, Admin)

**Endpoint:**
| Metot | Route | Yetki | Açıklama |
|-------|-------|-------|----------|
| POST | `/api/auth/login` | AllowAnonymous | Kullanıcı girişi, JWT token döner |

### 3. EmployeeService
Çalışan yönetimi servisi. CQRS, DDD ve Specification Pattern'ı tam olarak uygulayan örnek mikroservis.

**Endpoint:**
| Metot | Route | Yetki | Açıklama |
|-------|-------|-------|----------|
| POST | `/api/employee` | Admin, Manager | Yeni çalışan oluşturma |
| GET | `/api/employee` | Employee, Admin, Manager | Çalışan listesi (sayfalama destekli) |
| GET | `/api/employee/{id}` | Employee, Admin, Manager | Tekil çalışan getirme |
| GET | `/api/employee/public-info` | AllowAnonymous | Token gerektirmeyen açık endpoint |

---

## Proje Yapısı

```
dotnet8-microservices-boilerplate/
├── .editorconfig
├── .gitignore
├── docker-compose.yml
├── MicroservicesBoilerplate.sln
├── README.md
├── k8s/
│   ├── api-deployment.yaml
│   ├── auth-deployment.yaml
│   ├── gateway-deployment.yaml
│   ├── gateway-ingress.yaml
│   ├── infrastructure.yaml
│   └── vault-secret.yaml
└── src/
    ├── ApiGateway/
    │   └── Gateway.Api/                          ← YARP Reverse Proxy + JWT doğrulama
    │
    ├── BuildingBlocks/                           ← Tüm servislerce paylaşılan altyapı
    │   ├── BuildingBlocks.Domain/
    │   │   ├── Common/
    │   │   │   ├── AggregateRoot.cs              ← Domain Event koleksiyonunu yöneten temel sınıf
    │   │   │   ├── BaseEntity.cs                 ← Audit alanları (CreatedAt, UpdatedAt, IsDeleted, CreatedBy)
    │   │   │   └── IDomainEvent.cs               ← MediatR.INotification tabanlı domain event arayüzü
    │   │   ├── Exceptions/
    │   │   │   └── DomainExceptions.cs           ← NotFoundException, BadRequestException
    │   │   └── Models/
    │   │       ├── Result.cs                     ← Result<T> pattern (başarı/hata zarfı)
    │   │       └── Pagination/
    │   │           └── PagedResult.cs            ← TotalPages, HasNextPage, HasPreviousPage destekli
    │   │
    │   ├── BuildingBlocks.Application/
    │   │   ├── Behaviors/
    │   │   │   ├── ValidationBehavior.cs         ← FluentValidation MediatR pipeline
    │   │   │   ├── CachingBehavior.cs            ← Distributed cache MediatR pipeline
    │   │   │   └── KvkkCrudLoggingBehavior.cs    ← KVKK uyumlu kişisel veri maskeleme
    │   │   ├── Interfaces/
    │   │   │   ├── IGenericRepository.cs         ← Generic CRUD + Specification desteği
    │   │   │   ├── IUnitOfWork.cs                ← Transaction yönetimi
    │   │   │   ├── Caching/
    │   │   │   │   └── ICacheableQuery.cs        ← Cache bypass, TTL kontrollü sorgu arayüzü
    │   │   │   └── Security/
    │   │   │       └── ICurrentUserService.cs    ← JWT'den çözümlenen aktif kullanıcı bilgisi
    │   │   └── Specifications/
    │   │       ├── ISpecification.cs             ← Criteria, Include, OrderBy, Paging arayüzü
    │   │       └── BaseSpecification.cs          ← Specification temel sınıfı
    │   │
    │   ├── BuildingBlocks.Infrastructure/
    │   │   ├── Persistence/
    │   │   │   └── EfCoreRepositoryBase.cs       ← IGenericRepository generic implementasyonu
    │   │   ├── Interceptors/
    │   │   │   └── AuditableEntityInterceptor.cs ← CreatedBy/UpdatedBy otomatik doldurma
    │   │   ├── Extensions/
    │   │   │   └── ModelBuilderExtensions.cs     ← Global Soft Delete Query Filter (Reflection)
    │   │   └── Specifications/
    │   │       └── SpecificationEvaluator.cs     ← IQueryable üzerinde spec uygulama
    │   │
    │   ├── BuildingBlocks.Presentation/
    │   │   ├── Controllers/
    │   │   │   └── ApiControllerBase.cs          ← Mediator + TraceId + CreateResponse<T> yardımcıları
    │   │   ├── Middleware/
    │   │   │   ├── TraceIdMiddleware.cs           ← X-Trace-Id üretimi + Serilog LogContext
    │   │   │   ├── IdempotencyMiddleware.cs       ← X-Idempotency-Key ile tekrar eden istek koruması
    │   │   │   └── GlobalExceptionHandler.cs      ← ProblemDetails (RFC 7807) + TraceId
    │   │   ├── Models/
    │   │   │   └── ApiResponse.cs                 ← Standart API yanıt zarfı
    │   │   └── Services/
    │   │       └── CurrentUserService.cs          ← ICurrentUserService JWT claim implementasyonu
    │   │
    │   └── BuildingBlocks.Extensions/
    │       ├── AuthExtension.cs                   ← JWT Authentication yapılandırması
    │       ├── HttpClientBuilderExtensions.cs     ← Polly Retry + Circuit Breaker
    │       ├── MassTransitExtension.cs            ← RabbitMQ + EF Outbox yapılandırması
    │       ├── OpenTelemetryExtension.cs          ← Jaeger OTLP exporter
    │       ├── Messaging/
    │       │   ├── EmployeeCreatedIntegrationEvent.cs  ← Bounded context arası event kontratı
    │       │   └── EmployeeCreatedConsumer.cs           ← MassTransit consumer
    │       └── Vault/
    │           ├── VaultConnectionSettings.cs
    │           ├── VaultSecretLoader.cs            ← Vault KV v2 secret okuma
    │           └── VaultSecrets.cs                 ← DB, RabbitMQ, Graylog, Jaeger, JWT secret modelleri
    │
    ├── Core/
    │   ├── AuthService.Domain/
    │   │   ├── Entities/
    │   │   │   ├── User.cs                        ← AggregateRoot, private setter, Factory Pattern
    │   │   │   └── RefreshTokenEntity.cs          ← BaseEntity
    │   │   └── Enums/
    │   │       └── Role.cs                        ← User, Employee, Manager, Admin
    │   │
    │   ├── AuthService.Application/
    │   │   ├── DTOs/
    │   │   │   └── TokenResponseDto.cs
    │   │   ├── Features/
    │   │   │   └── Auth/Login/
    │   │   │       ├── LoginCommand.cs            ← IRequest<Result<TokenResponseDto>>
    │   │   │       └── LoginCommandHandler.cs     ← BCrypt.Verify ile güvenli doğrulama
    │   │   └── Interfaces/
    │   │       ├── IJwtProvider.cs
    │   │       ├── IUserRepository.cs
    │   │       └── IRefreshTokenRepository.cs
    │   │
    │   ├── EmployeeService.Domain/
    │   │   ├── Employee.cs                        ← AggregateRoot, Factory Pattern, Value Object
    │   │   ├── Events/
    │   │   │   └── EmployeeCreatedEvent.cs        ← IDomainEvent : INotification
    │   │   └── ValueObjects/
    │   │       └── IdentityNumber.cs              ← TCKN doğrulama (11 haneli)
    │   │
    │   └── EmployeeService.Application/
    │       ├── DTOs/
    │       │   └── EmployeeDto.cs
    │       ├── Features/
    │       │   └── Employees/
    │       │       ├── Commands/CreateEmployee/
    │       │       │   ├── CreateEmployeeCommand.cs
    │       │       │   ├── CreateEmployeeCommandHandler.cs   ← Mapperly mapper kullanır
    │       │       │   └── CreateEmployeeCommandValidator.cs ← FluentValidation
    │       │       └── Queries/GetEmployees/
    │       │           ├── GetEmployeesQuery.cs              ← Sayfalama parametreleri
    │       │           ├── GetEmployeesQueryHandler.cs
    │       │           └── GetEmployeesSpecification.cs      ← Filtreleme + sıralama + paging
    │       ├── Interfaces/
    │       │   └── IEmployeeRepository.cs
    │       └── Mappers/
    │           └── EmployeeMapper.cs              ← Riok.Mapperly (compile-time mapper)
    │
    ├── Infrastructure/
    │   ├── AuthService.Infrastructure/
    │   │   ├── AuthServiceRegistration.cs         ← DI: DbContext, UoW, Repositories, JWT, Interceptor
    │   │   ├── Persistence/
    │   │   │   └── AuthDbContext.cs               ← IUnitOfWork + Soft Delete Query Filter
    │   │   ├── Repositories/
    │   │   │   ├── UserRepository.cs
    │   │   │   └── RefreshTokenRepository.cs
    │   │   └── Services/
    │   │       └── JwtProvider.cs                 ← Repository Pattern ile RefreshToken yönetimi
    │   │
    │   └── EmployeeService.Infrastructure/
    │       ├── InfrastructureServiceRegistration.cs
    │       ├── Persistence/
    │       │   └── ApplicationDbContext.cs         ← IUnitOfWork + Soft Delete Query Filter
    │       └── Repositories/
    │           └── EmployeeRepository.cs
    │
    └── Presentation/
        ├── AuthService.Api/
        │   ├── Program.cs                         ← Serilog, CORS, RateLimiter, Swagger, OTel, KVKK
        │   └── Controllers/
        │       └── AuthController.cs
        │
        └── EmployeeService.Api/
            ├── Program.cs                         ← Serilog, CORS, RateLimiter, Swagger, OTel, KVKK
            └── Controllers/
                └── EmployeeController.cs          ← Rol bazlı yetkilendirme
```

---

## Kullanılan Enterprise Patternlar

### CQRS (Command Query Responsibility Segregation)
Yazma (Command) ve okuma (Query) işlemleri MediatR pipeline'ı üzerinden ayrılmıştır. Her istek MediatR handler tarafından işlenir ve `Result<T>` zarfı ile döner.

```
İstek Akışı:
Controller → MediatR.Send() → ValidationBehavior → CachingBehavior → KvkkLogging → Handler → Result<T>
```

### Result Pattern
İş mantığı **asla exception fırlatmaz**. Tüm sonuçlar `Result<T>` zarfı ile döner:
```csharp
Result<EmployeeDto>.Success(dto)
Result<EmployeeDto>.Failure(new Error("Employee.NotFound", "Çalışan bulunamadı."))
```

### Specification Pattern
Veritabanı sorguları `ISpecification<T>` üzerinden tanımlanır. `IQueryable` Application katmanına sızmaz:
- **Criteria**: Where koşulu
- **Includes**: Eager loading
- **OrderBy / OrderByDescending**: Sıralama
- **Skip / Take**: Sayfalama

### Repository + Unit of Work
- `IGenericRepository<T>`: GetById, GetAll, List (Spec), Count (Spec), Add, Update, Delete
- `IUnitOfWork`: SaveChangesAsync — tek bir transaction'da tüm değişiklikleri kaydeder
- Özel sorgular servis-spesifik repository arayüzlerine eklenir (ör: `IUserRepository.GetByUsernameAsync`)

### Domain Events
`IDomainEvent : MediatR.INotification` arayüzü ile tanımlanan olaylar `AggregateRoot` üzerinde tutulur:
```csharp
employee.AddDomainEvent(new EmployeeCreatedEvent(employee.Id, firstName, lastName));
```

### Transactional Outbox
MassTransit + Entity Framework Core Outbox Pattern ile çift-yazma (dual-write) problemi çözülür. Veritabanı ve mesaj kuyruğuna yazma aynı transaction içinde garanti altındadır.

---

## Altyapı Bileşenleri

### Güvenlik

| Bileşen | Açıklama |
|---------|----------|
| **HashiCorp Vault** | Tüm secret'lar (DB, RabbitMQ, JWT key, Graylog, Jaeger) Vault KV v2'den okunur. Kod içinde hardcoded secret yoktur. |
| **BCrypt** | Parolalar BCrypt ile hash'lenir ve doğrulanır. |
| **JWT** | Access Token (konfigüre edilebilir süre) + Refresh Token (konfigüre edilebilir gün) |
| **Rol Bazlı Yetkilendirme** | `[Authorize(Roles = "Admin,Manager")]` endpoint seviyesinde kontrol |

### Gözlemlenebilirlik (Observability)

| Bileşen | Açıklama |
|---------|----------|
| **Serilog + Graylog** | Yapısal loglama, merkezi log toplama (GELF UDP) |
| **OpenTelemetry + Jaeger** | Dağıtık izleme (distributed tracing), OTLP exporter |
| **TraceId Middleware** | Her isteğe `X-Trace-Id` atanır, Serilog context'e ve response header'a eklenir |
| **HealthChecks** | PostgreSQL, RabbitMQ, Vault, Graylog durum kontrolleri + HealthChecks UI Dashboard |

### Dayanıklılık (Resilience)

| Bileşen | Açıklama |
|---------|----------|
| **Polly Retry** | HTTP çağrılarında 3 deneme, exponential backoff |
| **Polly Circuit Breaker** | 5 ardışık hata sonrası 30 saniye devre kesici |
| **Idempotency Middleware** | `X-Idempotency-Key` header'ı ile POST/PUT/PATCH tekrarlanan isteklerde cache'ten yanıt |
| **Rate Limiter** | IP başına dakikada 100 istek limiti |

### Veri Katmanı

| Bileşen | Açıklama |
|---------|----------|
| **PostgreSQL** | İlişkisel veritabanı |
| **EF Core 8** | ORM, AuditableEntityInterceptor ile otomatik audit |
| **Global Soft Delete** | Reflection ile tüm BaseEntity türlerine `!e.IsDeleted` query filter uygulanır |
| **API Versioning** | Header tabanlı (`x-api-version`) sürümleme |

---

## Hızlı Başlangıç

### Gereksinimler
- .NET 8 SDK
- Docker & Docker Compose

### 1. Altyapı Servislerini Başlat
```bash
docker-compose up -d
```
Bu komut aşağıdaki servisleri ayağa kaldırır:
- **PostgreSQL** (5432)
- **RabbitMQ** (5672 / Management: 15672)
- **HashiCorp Vault** (8200) — Otomatik secret seed
- **Graylog** (9000 / GELF: 12201) + Elasticsearch + MongoDB
- **Jaeger** (16686 / OTLP: 4317)

### 2. Mikroservisleri Çalıştır
```bash
dotnet run --project src/Presentation/AuthService.Api
dotnet run --project src/Presentation/EmployeeService.Api
dotnet run --project src/ApiGateway/Gateway.Api
```

### 3. Swagger ile Test Et
- AuthService: `http://localhost:{port}/swagger`
- EmployeeService: `http://localhost:{port}/swagger`

### 4. Kubernetes'e Deploy Et
```bash
kubectl apply -f k8s/infrastructure.yaml
kubectl apply -f k8s/vault-secret.yaml
kubectl apply -f k8s/auth-deployment.yaml
kubectl apply -f k8s/api-deployment.yaml
kubectl apply -f k8s/gateway-deployment.yaml
kubectl apply -f k8s/gateway-ingress.yaml
```

---

## Yeni Mikroservis Ekleme Rehberi

Yeni bir mikroservis eklemek için aşağıdaki 4 adımlı yapıyı takip edin:

### Adım 1: Domain Katmanı
`src/Core/YeniService.Domain/` klasörü oluşturun:
- Entity sınıfınızı `AggregateRoot` veya `BaseEntity`'den türetin
- Value Object'leri `ValueObjects/` klasöründe tanımlayın
- Domain Event'leri `Events/` klasöründe `IDomainEvent` arayüzü ile oluşturun

### Adım 2: Application Katmanı
`src/Core/YeniService.Application/` klasörü oluşturun:
- `Features/` altında Feature Folder yapısında Command/Query tanımlayın
- `Interfaces/` altında repository arayüzünü `IGenericRepository<T>` üzerinden türetin
- `DTOs/` altında DTO sınıflarınızı oluşturun
- `Mappers/` altında Mapperly ile compile-time mapper tanımlayın

### Adım 3: Infrastructure Katmanı
`src/Infrastructure/YeniService.Infrastructure/` klasörü oluşturun:
- `Persistence/` altında DbContext'i `IUnitOfWork` ile implemente edin
- `OnModelCreating` içinde `modelBuilder.ApplyGlobalQueryFilter()` çağırın
- `Repositories/` altında `EfCoreRepositoryBase<T, TContext>` ile repository oluşturun
- `ServiceRegistration` sınıfında DbContext, UoW, Repository DI kayıtlarını yapın

### Adım 4: Presentation Katmanı
`src/Presentation/YeniService.Api/` klasörü oluşturun:
- Controller'ı `ApiControllerBase`'den türetin
- `Program.cs`'de Serilog, CORS, RateLimiter, Swagger, MediatR, HealthChecks, OpenTelemetry, MassTransit yapılandırmalarını ekleyin
- MediatR pipeline'ına `ValidationBehavior`, `CachingBehavior`, `KvkkCrudLoggingBehavior` ekleyin

---

## MediatR Pipeline Sırası

```
İstek → ValidationBehavior (FluentValidation)
      → CachingBehavior (ICacheableQuery ise cache kontrol)
      → KvkkCrudLoggingBehavior (Create/Update/Delete audit)
      → Handler (İş mantığı)
      → Result<T> yanıtı
```

---

## API Yanıt Formatı

### Başarılı Yanıt
```json
{
  "data": { ... },
  "errors": [],
  "traceId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "isSuccess": true
}
```

### Hatalı Yanıt
```json
{
  "data": null,
  "errors": ["Geçersiz kullanıcı adı veya şifre."],
  "traceId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "isSuccess": false
}
```

### Validation Hatası (ProblemDetails — RFC 7807)
```json
{
  "status": 400,
  "title": "Validation Error",
  "detail": "One or more validation errors occurred.",
  "extensions": {
    "traceId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "errors": {
      "FirstName": ["'First Name' must not be empty."],
      "IdentityNumber": ["'Identity Number' must be 11 characters in length."]
    }
  }
}
```

---

## Kodlama Standartları

- `var` keyword kullanılmaz, tüm tipler açık (explicit) yazılır
- Yorum satırı (`//`, `/* */`, `///`) kullanılmaz
- Property setter'lar Domain Entity'lerde `private set` olmalıdır
- Değişiklikler yalnızca Domain metotları üzerinden yapılır (Encapsulation)
- Exception yerine `Result<T>` pattern kullanılır
- Tüm Controller'lar `ApiControllerBase`'den türer

---

## Teknoloji Yığını

| Kategori | Teknoloji |
|----------|-----------|
| Framework | .NET 8 |
| ORM | Entity Framework Core 8 |
| Veritabanı | PostgreSQL 15 |
| Mesajlaşma | RabbitMQ + MassTransit |
| API Gateway | YARP |
| Authentication | JWT Bearer |
| Parola Güvenliği | BCrypt |
| Validation | FluentValidation |
| CQRS | MediatR 12.4 |
| Mapping | Riok.Mapperly |
| Loglama | Serilog + Graylog |
| Tracing | OpenTelemetry + Jaeger |
| Resilience | Polly |
| Secret Management | HashiCorp Vault |
| Containerization | Docker + Docker Compose |
| Orchestration | Kubernetes |
| API Versioning | Asp.Versioning (Header-based) |
