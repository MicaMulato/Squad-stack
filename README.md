# DigitalArs — Billetera Virtual (Backend API)

> **Proyecto:** Billetera Virtual / Digital Wallet  
> **Programa:** Aceleración Técnica en **Alchemie Acceleration Tech**  
> **Equipo:** **Squad-stack** — Emmanuel, Andrés, Micaela y Máximo  
> **Stack Principal:** .NET 10 | ASP.NET Core Web API | Entity Framework Core | SQL Server | ASP.NET Core Identity | JWT Bearer  

---

## 1. Descripción del Proyecto

**DigitalArs** es una solución fintech integral construida con arquitectura limpia (*Clean Architecture*) y estándares empresariales. Proporciona una API RESTful de alto rendimiento para la gestión financiera segura de cuentas bancarias virtuales, procesamiento de transferencias atómicas entre usuarios, depósitos con comprobante, simulación y constitución de plazos fijos de inversión, emisión y administración de tarjetas (virtuales y físicas), y un panel de control administrativo con control de accesos basado en roles (*RBAC*).

---

## 2. Arquitectura del Sistema (Clean Architecture)

El backend está organizado en cuatro capas estrictamente desacopladas para garantizar alta mantenibilidad, testabilidad y separación de responsabilidades:

```
DigitalArs.slnx
├── DigitalArs.Domain/           # Capa de Dominio: Entidades de negocio puras, enums y reglas independientes
├── DigitalArs.Application/      # Capa de Aplicación: Interfaces de servicios, DTOs, validaciones y lógica de negocio
├── DigitalArs.Infrastructure/   # Capa de Infraestructura: Entity Framework Core, ApplicationDbContext, Repositorios, Servicios y Migraciones
├── DigitalArs.Api/              # Capa de Presentación: Controllers REST, Middlewares, Filtros, Swagger y Program.cs
└── DigitalArs.UnitTests/        # Pruebas Unitarias: Tests automatizados con xUnit, Moq y FluentAssertions
```

### Inyección de Dependencias Modular
La configuración de servicios se encuentra desacoplada mediante métodos de extensión:
- `services.AddApplication()`: Registra validadores y configuraciones de aplicación.
- `services.AddInfrastructure(configuration)`: Registra `ApplicationDbContext`, Identity, Repositorios genéricos, Unit of Work y servicios de infraestructura (`AccountService`, `TransactionService`, `FixedTermDepositService`, `CardService`, `JwtTokenGenerator`).

---

## 3. Stack Tecnológico

| Componente | Tecnología | Versión | Rol en la Solución |
| :--- | :--- | :---: | :--- |
| **Runtime & SDK** | [.NET](https://dotnet.microsoft.com/) | `10.0` | Framework de ejecución principal de alto rendimiento |
| **Web Framework** | [ASP.NET Core](https://learn.microsoft.com/aspnet/core) | `10.0` | Creación de controladores RESTful, middlewares y pipeline HTTP |
| **ORM** | [Entity Framework Core](https://learn.microsoft.com/ef/core) | `10.0` | Mapeo objeto-relacional con enfoque *Code First* y Fluent API |
| **Motor de Base de Datos** | Microsoft SQL Server / LocalDB | `2022+` | Base de datos relacional con integridad referencial e índices optimizados |
| **Seguridad e Identidad** | ASP.NET Core Identity | `10.0` | Gestión de usuarios, roles, claims y hash seguro de contraseñas |
| **Autenticación** | JWT (JSON Web Tokens) | `Bearer` | Tokens firmados criptográficamente con `HMAC-SHA256` |
| **Documentación API** | OpenAPI / Swagger UI | `Swashbuckle 7.0` | Explorador interactivo y documentación viva de endpoints |
| **Testing Automatizado** | xUnit & Moq | `2.9+` | Suite de pruebas unitarias y mocks para lógica de servicios |

---

## 4. Requisitos Previos

Antes de clonar y ejecutar el proyecto, asegúrese de tener instalado:
1. **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** (o superior).
2. **Microsoft SQL Server** (LocalDB incluido con Visual Studio, SQL Server Express o instancia de SQL Server en Docker).
3. **IDE / Editor:** Visual Studio 2022+, VS Code (con C# Dev Kit), Rider o Kiro IDE.
4. **Herramienta EF Core CLI:**
   ```bash
   dotnet tool install --global dotnet-ef
   ```

---

## 5. Instalación y Puesta en Marcha

### Paso 1: Clonar el Repositorio
```bash
git clone https://github.com/MicaMulato/Squad-stack.git
cd Squad-stack
```

### Paso 2: Configurar la Cadena de Conexión y JWT
Verifique la cadena de conexión en `DigitalArs.Api/appsettings.json` o configure su entorno local en `DigitalArs.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=DigitalArsDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "SecretKey": "CAMBIA_ESTA_CLAVE_POR_UNA_PROPIA_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "DigitalArs.Api",
    "Audience": "DigitalArsUsers",
    "ExpirationMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173"
    ]
  },
  "DepositSettings": {
    "MaxAmountPerOperation": 1000000
  }
}
```

### Paso 3: Aplicar Migraciones de Base de Datos y Seed Data
Ejecute el siguiente comando para generar la base de datos `DigitalArsDb` y precargar los datos de prueba iniciales:

```bash
dotnet ef database update --project DigitalArs.Infrastructure --startup-project DigitalArs.Api
```

### Paso 4: Ejecutar la API
```bash
dotnet run --project DigitalArs.Api
```

La API se iniciará de forma predeterminada en:
- **API URL:** `http://localhost:5065` o `https://localhost:7065`
- **Swagger UI:** `http://localhost:5065/swagger`

---

## 6. Credenciales de Prueba (Data Seeding)

La base de datos se inicializa automáticamente con usuarios, cuentas activas, saldos e historial de movimientos:

| Rol | Nombre | Email | Contraseña | Saldo Inicial | Estado |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **Admin** | Administrador DigitalArs | `admin@digitalars.com` | `Admin123!` | $500.000,00 | Activo |
| **User** | Roberto Carlos | `robercarlos3@gmail.com` | `Roberto1!` | $260.000,00 | Activo |
| **User** | Mohammed Khan | `mokha@gmail.com` | `Mohammed1!` | $185.000,50 | Activo |
| **User** | Alejandro Silva | `alejandro.silva@digitalars.com` | `User123!` | $45.230,50 | Activo |
| **User** | Micaela Mulato | `micaela.mulato@digitalars.com` | `User123!` | $320.000,00 | Activo |
| **User** | Emmanuel Torres | `emmanuel.torres@digitalars.com` | `User123!` | $410.000,00 | Activo |

> **Seguridad:** Todas las contraseñas están procesadas con el algoritmo `PBKDF2` con salt criptográfico mediante `PasswordHasher<User>` de ASP.NET Core Identity.

---

## 7. Diagrama Entidad-Relación (ER Diagram)

El modelo de datos relacional modela integralmente el ciclo financiero de la billetera:

```mermaid
erDiagram
    ROLE ||--o{ USER : "posee (1:N)"
    USER ||--|| ACCOUNT : "titular de (1:1)"
    ACCOUNT ||--o{ TRANSACTION : "origen (1:N)"
    ACCOUNT ||--o{ TRANSACTION : "destino (1:N)"
    ACCOUNT ||--o{ FIXED_TERM_DEPOSIT : "invierte en (1:N)"
    ACCOUNT ||--o{ CARD : "emite (1:N)"

    ROLE {
        int Id PK
        string Name
        string NormalizedName
        string Description
        string ConcurrencyStamp
    }

    USER {
        int Id PK
        string FirstName
        string LastName
        string Email UK
        string UserName
        string PasswordHash
        int RoleId FK
        bool IsDeleted
        datetime CreatedAt
        string SecurityStamp
        string ConcurrencyStamp
    }

    ACCOUNT {
        int Id PK
        int UserId FK,UK
        decimal Money
        bool IsBlocked
        datetime CreatedAt
    }

    TRANSACTION {
        int Id PK
        int AccountId FK
        int ToAccountId FK "nullable"
        decimal Amount
        int Type "1=Deposit, 2=TransferReceived, 3=TransferSent"
        string Concept
        datetime Date
    }

    FIXED_TERM_DEPOSIT {
        int Id PK
        int AccountId FK
        decimal Amount
        decimal InterestRate
        int DurationDays
        datetime CreationDate
        datetime ClosingDate
        decimal InterestEarned
        decimal FinalAmount
        int Status "1=Active, 2=Finished, 3=Cancelled"
    }

    CARD {
        int Id PK
        int AccountId FK
        string HolderName
        string CardNumber "16 digits"
        string SecurityCode "3 digits"
        datetime ExpirationDate
        int Type "1=Virtual, 2=Physical"
        bool IsActive
        bool IsFrozen
        datetime CreatedAt
    }
```

---

## 8. Catálogo de Módulos y Endpoints Principales

### 8.1. Autenticación (`/api/auth`)
- `POST /api/auth/login`: Autentica credenciales y emite token JWT con claims de usuario y rol.
- `POST /api/auth/register`: Registro de nuevos usuarios con creación automática de su `Account` bancaria inicial.

### 8.2. Cuentas y Saldo (`/api/accounts`)
- `GET /api/accounts/balance`: Consulta de saldo disponible en tiempo real para el usuario autenticado.
- `POST /api/accounts/deposit`: Acreditación de fondos propios en cuenta con registro de auditoría.
- `GET /api/accounts/me`: Información detallada de la cuenta y titular.

### 8.3. Transferencias y Transacciones (`/api/transactions`)
- `POST /api/transactions/transfer`: Transferencia atómica entre cuentas con verificación de saldo, cuenta activa y generación de dos asientos contables vinculados.
- `GET /api/transactions/history`: Historial paginado con filtros por fecha, tipo de operación y concepto.
- `GET /api/transactions/{id}`: Detalle de una transacción específica para emisión de comprobante.

### 8.4. Inversiones a Plazo Fijo (`/api/fixedterm`)
- `POST /api/fixedterm`: Constitución de plazo fijo debitando saldo disponible según TNA (Tasa Nominal Anual) y plazo seleccionado (mínimo 30 días).
- `GET /api/fixedterm`: Listado de plazos fijos del usuario (activos, finalizados y cancelados).
- `POST /api/fixedterm/{id}/cancel`: Cancelación anticipada o cierre de plazo fijo con reintegro de fondos a la cuenta.

### 8.5. Tarjetas Virtuales y Físicas (`/api/cards`)
- `GET /api/cards`: Obtiene todas las tarjetas asociadas a la cuenta del usuario.
- `POST /api/cards`: Emisión instantánea de nueva tarjeta (Virtual o Física) con numeración formateada y CVV seguro.
- `POST /api/cards/{id}/toggle-freeze`: Congelamiento y descongelamiento temporal inmediato de tarjeta.
- `DELETE /api/cards/{id}`: Baja lógica / cancelación de tarjeta.

### 8.6. Administración y Auditoría (`/api/users` & `/api/admin`)
- `GET /api/users`: Listado paginado de usuarios para administradores (`Role: Admin`).
- `PUT /api/users/{id}/block`: Bloqueo/desbloqueo de cuentas por seguridad o mora.
- `PUT /api/users/{id}/role`: Asignación y elevación de roles de usuario.
- `DELETE /api/users/{id}`: Baja lógica (*soft delete*) de usuarios del sistema.

---

## 9. Pruebas Automatizadas

El proyecto cuenta con un proyecto dedicado de pruebas unitarias (`DigitalArs.UnitTests`) que valida la lógica de negocio central (depósitos, transferencias sin saldo suficiente, validaciones de tarjetas, cálculo de intereses en plazos fijos).

Para ejecutar toda la suite de tests:

```bash
dotnet test
```

---

## 10. Seguridad y Buenas Prácticas

- **Sin Secretos en Código:** La configuración sensible utiliza cadenas parametrizadas en `appsettings.json`, variables de entorno y soporte para `dotnet user-secrets`.
- **Protección CORS:** Configuración estricta de orígenes permitidos para consumo seguro desde la SPA de React (`http://localhost:5173`).
- **Consultas Optimizadas:** Uso sistemático de `.AsNoTracking()` en endpoints de lectura, índices no agrupados sobre `AccountId`, `Date`, `UserId` y `Email`.
- **Integridad Transaccional:** Transacciones de base de datos con rollback automático ante excepciones en transferencias de fondos y débitos de plazos fijos.

---

## 11. Equipo de Desarrollo (Squad-stack)

- **Emmanuel Torres**
- **Andrés**
- **Micaela Mulato**
- **Máximo Porretti**
