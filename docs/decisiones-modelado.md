# Justificacion de Indices y Decisiones de Modelado

> **Diagrama ER:** El diagrama entidad-relacion del modelo fue generado desde la base de datos con SQL Server Management Studio (SSMS). Ver `docs/diagrama-er.pdf`.

## 1. Indices creados

### 1.1 IX_AspNetUsers_Email (Unique, filtrado)

```csharp
builder.HasIndex(u => u.Email)
    .IsUnique()
    .HasFilter("[Email] IS NOT NULL");
```

**Justificacion:**

- El email es el identificador principal de login. Cada autenticacion ejecuta un `WHERE Email = @email`, por lo que sin indice esto seria un table scan en cada intento de login.
- Se marca como `UNIQUE` para garantizar integridad a nivel de BD (no dos usuarios con el mismo email).
- El filtro `IS NOT NULL` es necesario porque SQL Server no permite valores duplicados de NULL en indices unicos sin filtro.
- **Impacto estimado:** Reduce la busqueda de O(n) a O(log n) en la tabla mas consultada del sistema.

### 1.2 IX_Accounts_UserId (Unique)

```csharp
builder.HasIndex(a => a.UserId)
    .IsUnique();
```

**Justificacion:**

- Refuerza la relacion 1:1 entre User y Account a nivel de base de datos. Sin este indice unico, EF Core permitiria multiples Accounts para un mismo User.
- Acelera los JOINs `User → Account` que se ejecutan en practicamente todas las operaciones de consulta de saldo y transacciones.
- Al ser unico, SQL Server usa un Index Seek en vez de Index Scan para buscar la cuenta de un usuario.

### 1.3 IX_Transactions_Date

```csharp
builder.HasIndex(t => t.Date);
```

**Justificacion:**

- Las consultas de historial de transacciones filtran por rango de fechas (`WHERE Date BETWEEN @inicio AND @fin`). Sin indice, cada consulta de historial requeriria escanear toda la tabla Transactions.
- A medida que la tabla crece (es la de mayor volumen esperado), este indice es critico para mantener tiempos de respuesta aceptables.
- Se eligio un indice no-unico porque multiples transacciones pueden ocurrir en el mismo instante.

### 1.4 IX_Transactions_AccountId (FK index)

EF Core genera automaticamente un indice en `AccountId` por ser Foreign Key.

**Justificacion:**

- Necesario para resolver eficientemente "todas las transacciones de una cuenta" (`WHERE AccountId = @id`), que es la consulta mas frecuente del modulo de movimientos.
- Sin este indice, cada vez que se consulta el historial de una cuenta, SQL Server haria un full scan de Transactions.

### 1.5 IX_Transactions_ToAccountId (FK index)

EF Core genera automaticamente un indice en `ToAccountId` por ser Foreign Key.

**Justificacion:**

- Permite buscar eficientemente las transferencias recibidas por una cuenta (`WHERE ToAccountId = @id`).
- Tambien necesario para validar integridad referencial sin bloqueos en la tabla padre (Accounts) durante inserts.

---

## 2. Decisiones de modelado

### 2.1 Identity con PK de tipo `int` en vez de `string` (GUID)

**Decision:** `IdentityUser<int>` y `IdentityRole<int>`

**Razones:**

- **Rendimiento en JOINs:** Las claves enteras de 4 bytes son significativamente mas rapidas en comparaciones que strings de 36 caracteres (GUID).
- **Menor almacenamiento:** Cada FK ocupa 4 bytes en vez de 36+ bytes, reduciendo el tamaño de indices y paginas de datos.
- **Indices mas compactos:** Mas registros por pagina de indice = menos I/O en lecturas.
- **Trade-off:** Se pierde la ventaja de GUIDs para sistemas distribuidos, pero esta aplicacion usa un unico SQL Server.

### 2.2 FK directa `User.RoleId` ademas de `AspNetUserRoles`

**Decision:** Agregar `RoleId` como propiedad directa en User, apuntando a Role.

**Razones:**

- Identity usa una tabla intermedia (`AspNetUserRoles`) para la relacion M:N entre Users y Roles. Sin embargo, en DigitalArs cada usuario tiene exactamente un rol.
- La FK directa evita un JOIN adicional con la tabla intermedia en consultas frecuentes como "obtener usuario con su rol".
- **Trade-off:** Existe redundancia con `AspNetUserRoles`, pero simplifica enormemente las consultas y el codigo.

### 2.3 Relacion User-Account como 1:1 (no 1:N)

**Decision:** Indice unico en `Accounts.UserId` para forzar una sola cuenta por usuario.

**Razones:**

- El negocio define que cada usuario tiene exactamente una cuenta (billetera).
- Simplifica la logica de transferencias: no hay ambiguedad sobre "cual cuenta" del usuario.
- Si en el futuro se necesitan multiples cuentas, se remueve el constraint unico y se ajusta la navegacion.

### 2.4 Borrado en cascada deshabilitado (Restrict)

**Decision:** `OnDelete(DeleteBehavior.Restrict)` en todas las relaciones.

**Razones:**

- **Prevencion de ciclos:** SQL Server rechaza multiples cascadas que convergen en la misma tabla. `Transaction` tiene dos FKs hacia `Account` (`AccountId` y `ToAccountId`); si ambas fueran CASCADE, SQL Server lanza error.
- **Seguridad de datos:** En una billetera virtual, borrar accidentalmente un usuario no debe eliminar en cascada sus transacciones (historial financiero auditado).
- **Soft delete preferido:** El campo `User.IsDeleted` permite "eliminar" usuarios sin perder integridad referencial.

### 2.5 Montos con `decimal(18,2)` en vez de `float` o `double`

**Decision:** Precision `HasPrecision(18, 2)` en `Account.Money` y `Transaction.Amount`.

**Razones:**

- `float`/`double` usan representacion IEEE 754 con errores de redondeo (ej: 0.1 + 0.2 != 0.3).
- `decimal` es un tipo de punto fijo que garantiza precision exacta en operaciones monetarias.
- `(18, 2)` permite hasta 16 digitos enteros y 2 decimales, suficiente para montos en pesos argentinos.

### 2.6 Enum `TransactionType` almacenado como `int`

**Decision:** EF Core almacena el enum como entero en la columna `Type`.

**Razones:**

- Almacenar como `int` (4 bytes) es mas eficiente que `nvarchar` para filtros y agrupaciones.
- Los valores explicitos (`Deposit = 1, TransferIn = 2, TransferOut = 3`) evitan que reordenar el enum cambie los datos existentes.
- La logica de negocio interpreta el valor en la capa de aplicacion; la BD solo almacena el numero.

### 2.7 `BaseEntity` como clase abstracta con `Id`

**Decision:** Todas las entidades propias (Account, Transaction) heredan de `BaseEntity` con PK `int Id`.

**Razones:**

- Permite el repositorio generico `IBaseRepository<T> where T : BaseEntity` sin duplicar codigo por entidad.
- Separa las entidades de dominio (nuestras) de las de Identity (User, Role) que tienen su propia jerarquia.
- Convencional y predecible: todo el equipo sabe que cualquier entidad tiene `.Id`.
