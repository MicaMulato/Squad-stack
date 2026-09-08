# Justificación de Índices y Decisiones de Modelado

> **Diagrama ER:** El diagrama entidad-relación del modelo fue generado desde la base de datos con SQL Server Management Studio (SSMS). Ver `docs/diagrama-er.pdf` y `docs/diagrama-er.md`.

## 1. Índices Creados

### 1.1 IX_AspNetUsers_Email (Unique, filtrado)

```csharp
builder.HasIndex(u => u.Email)
    .IsUnique()
    .HasFilter("[Email] IS NOT NULL");
```

**Justificación:**
- El email es el identificador principal de login. Cada autenticación ejecuta un `WHERE Email = @email`, por lo que sin índice esto sería un table scan en cada intento de login.
- Se marca como `UNIQUE` para garantizar integridad a nivel de BD (no dos usuarios con el mismo email).
- El filtro `IS NOT NULL` es necesario porque SQL Server no permite valores duplicados de NULL en índices únicos sin filtro.
- **Impacto estimado:** Reduce la búsqueda de O(n) a O(log n) en la tabla más consultada del sistema.

### 1.2 IX_Accounts_UserId (Unique)

```csharp
builder.HasIndex(a => a.UserId)
    .IsUnique();
```

**Justificación:**
- Refuerza la relación 1:1 entre User y Account a nivel de base de datos. Sin este índice único, EF Core permitiría múltiples Accounts para un mismo User.
- Acelera los JOINs `User → Account` que se ejecutan en prácticamente todas las operaciones de consulta de saldo y transacciones.
- Al ser único, SQL Server usa un Index Seek en vez de Index Scan para buscar la cuenta de un usuario.

### 1.3 IX_Accounts_Cvu (Unique)

```csharp
builder.HasIndex(a => a.Cvu)
    .IsUnique();
```

**Justificación:**
- El CVU (Clave Virtual Uniforme) de 22 dígitos es el identificador bancario interoperable de la cuenta.
- El índice único garantiza que ninguna cuenta en la plataforma pueda tener un CVU duplicado y permite resolución O(1) en búsquedas de destinatarios para transferencias (`GET /api/accounts/lookup?query={cvu}`).

### 1.4 IX_Accounts_Alias (Unique)

```csharp
builder.HasIndex(a => a.Alias)
    .IsUnique();
```

**Justificación:**
- El Alias alfanumérico es la vía principal de transferencia entre billeteras virtuales argentinas.
- El índice único previene colisiones al editar o crear alias, y permite una búsqueda ultrarrápida (Index Seek) en la validación previa a transferir fondos (`GET /api/accounts/lookup?query={alias}`).

### 1.5 IX_Transactions_Date

```csharp
builder.HasIndex(t => t.Date);
```

**Justificación:**
- Las consultas de historial de transacciones filtran por rango de fechas (`WHERE Date BETWEEN @inicio AND @fin`). Sin índice, cada consulta de historial requeriría escanear toda la tabla Transactions.
- A medida que la tabla crece (es la de mayor volumen esperado), este índice es crítico para mantener tiempos de respuesta aceptables.
- Se eligió un índice no-único porque múltiples transacciones pueden ocurrir en el mismo instante.

### 1.6 IX_Transactions_AccountId (FK index)

EF Core genera automáticamente un índice en `AccountId` por ser Foreign Key.

**Justificación:**
- Necesario para resolver eficientemente "todas las transacciones de una cuenta" (`WHERE AccountId = @id`), que es la consulta más frecuente del módulo de movimientos.
- Sin este índice, cada vez que se consulta el historial de una cuenta, SQL Server haría un full scan de Transactions.

### 1.7 IX_Transactions_ToAccountId (FK index)

EF Core genera automáticamente un índice en `ToAccountId` por ser Foreign Key.

**Justificación:**
- Permite buscar eficientemente las transferencias recibidas por una cuenta (`WHERE ToAccountId = @id`).
- También necesario para validar integridad referencial sin bloqueos en la tabla padre (Accounts) durante inserts.

### 1.8 IX_Cards_AccountId & IX_FixedTermDeposits_AccountId

**Justificación:**
- Optimizan el filtrado de tarjetas y plazos fijos asociados a una cuenta bancaria específica (`WHERE AccountId = @accountId`).

---

## 2. Decisiones de Modelado

### 2.1 Identificadores Bancarios Interoperables: CVU y Alias (HU-31)
- **CVU (22 dígitos)**: Se genera automáticamente al registrarse la cuenta con prefijo bancario simulado `0000003100010` + 9 dígitos aleatorios garantizados sin colisión. Es inmutable.
- **Alias (alfanumérico)**: Se genera inicialmente con formato `nombre.apellido.ars` (o con sufijo numérico ante colisión). Es editable por el usuario con validación de unicidad en tiempo real.

### 2.2 Identity con PK de tipo `int` en vez de `string` (GUID)
- **Rendimiento en JOINs:** Las claves enteras de 4 bytes son significativamente más rápidas en comparaciones que strings de 36 caracteres (GUID).
- **Menor almacenamiento:** Cada FK ocupa 4 bytes en vez de 36+ bytes, reduciendo el tamaño de índices y páginas de datos.

### 2.3 FK directa `User.RoleId` además de `AspNetUserRoles`
- La FK directa evita un JOIN adicional con la tabla intermedia en consultas frecuentes como "obtener usuario con su rol".

### 2.4 Relación User-Account como 1:1 (no 1:N)
- El negocio define que cada usuario tiene exactamente una cuenta (billetera).

### 2.5 Borrado en cascada deshabilitado (Restrict)
- En una billetera virtual, borrar accidentalmente un usuario no debe eliminar en cascada sus transacciones (historial financiero auditado).
- El campo `User.IsDeleted` permite "eliminar" usuarios mediante baja lógica (*Soft Delete*) sin perder integridad referencial.

### 2.6 Montos con `decimal(18,2)` en vez de `float` o `double`
- `decimal` es un tipo de punto fijo que garantiza precisión exacta en operaciones monetarias.
