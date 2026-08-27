# DigitalArs - Billetera Virtual

API REST desarrollada en C# con .NET 10 que permite realizar transacciones de dinero entre usuarios, depositos en cuenta, y funcionalidades de administracion.

## Tecnologias

- .NET 10 / ASP.NET Core
- Entity Framework Core (Code First)
- SQL Server
- ASP.NET Core Identity (autenticacion y roles)
- JWT (JSON Web Tokens)
- Swagger / OpenAPI

## Arquitectura

El proyecto sigue una arquitectura en capas (Clean Architecture):

```
DigitalArs.Api/              → Capa de presentacion (controllers, Program.cs)
DigitalArs.Application/      → Capa de aplicacion (interfaces, servicios)
DigitalArs.Domain/           → Capa de dominio (entidades, enums)
DigitalArs.Infrastructure/   → Capa de infraestructura (DbContext, repositorios, migraciones)
```

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB o instancia local)
- Visual Studio 2022+ / VS Code / Kiro

## Configuracion

1. Clonar el repositorio:

```bash
git clone https://github.com/MicaMulato/Squad-stack.git
cd Squad-stack
```

2. Configurar la connection string en `DigitalArs.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DigitalArsDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

3. Aplicar la migracion para crear la base de datos:

```bash
dotnet ef database update --project DigitalArs.Infrastructure --startup-project DigitalArs.Api
```

4. Ejecutar la aplicacion:

```bash
dotnet run --project DigitalArs.Api
```

## Credenciales de prueba

La base de datos se precarga con los siguientes usuarios de prueba:

| Rol       | Email                  | Password   | Saldo inicial |
| --------- | ---------------------- | ---------- | ------------- |
| **Admin** | admin@digitalars.com   | Admin123!  | $500.000,00   |
| User      | robercarlos3@gmail.com | Roberto1!  | $260.000,00   |
| User      | mokha@gmail.com        | Mohammed1! | $185.000,50   |

> Las contrasenas estan hasheadas en la base de datos usando el PasswordHasher de ASP.NET Core Identity.

## Estructura de la base de datos

### Entidades principales

- **User** (hereda de IdentityUser): Usuarios del sistema con roles asignados
- **Role** (hereda de IdentityRole): Admin y User
- **Account**: Cuenta bancaria del usuario (relacion 1:1 con User)
- **Transaction**: Movimientos de dinero (depositos y transferencias)

### Tipos de transaccion

| Enum        | Valor | Descripcion               |
| ----------- | ----- | ------------------------- |
| Deposit     | 1     | Deposito en cuenta propia |
| TransferIn  | 2     | Transferencia recibida    |
| TransferOut | 3     | Transferencia enviada     |

## Patrones implementados

- **Repository Pattern**: Acceso a datos generico y desacoplado (`IRepository<T>`)
- **Unit of Work**: Gestion transaccional coordinada (`IUnitOfWork`)
- **Code First**: Modelado de BD desde clases C#
- **Fluent API**: Configuracion de relaciones, indices y restricciones

## Equipo

| Integrante | Historias de usuario                                                              |
| ---------- | --------------------------------------------------------------------------------- |
| Micaela    | HU-01 (Estructura), HU-04 (Migracion)                                             |
| Maximo     | HU-02 (Entidades dominio)                                                         |
| Emmanuel   | HU-03 (DbContext), HU-05 (Seeding), HU-06 (Repository/UoW), HU-07 (Documentacion) |

## Scripts utiles

```bash
# Compilar la solucion
dotnet build

# Agregar nueva migracion
dotnet ef migrations add NombreMigracion --project DigitalArs.Infrastructure --startup-project DigitalArs.Api

# Aplicar migraciones
dotnet ef database update --project DigitalArs.Infrastructure --startup-project DigitalArs.Api

# Generar script SQL
dotnet ef migrations script --project DigitalArs.Infrastructure --startup-project DigitalArs.Api -o db/schema.sql
```
