# Consultas LINQ y SQL Generado por EF Core

Este documento presenta 4 consultas LINQ representativas del sistema DigitalArs junto con el SQL que EF Core genera internamente y los resultados reales obtenidos contra la base de datos `DigitalArsDb`.

---

## Consulta 1: Obtener usuario con su cuenta y rol

**Caso de uso:** Mostrar el perfil completo del usuario logueado (datos personales + saldo + rol).

### LINQ

```csharp
var usuario = await _context.Users
    .Include(u => u.Account)
    .Include(u => u.Role)
    .FirstOrDefaultAsync(u => u.Id == userId);
```

### SQL generado

```sql
SELECT TOP(1) [u].[Id], [u].[AccessFailedCount], [u].[ConcurrencyStamp],
       [u].[CreatedAt], [u].[Email], [u].[EmailConfirmed], [u].[FirstName],
       [u].[IsDeleted], [u].[LastName], [u].[LockoutEnabled], [u].[LockoutEnd],
       [u].[NormalizedEmail], [u].[NormalizedUserName], [u].[PasswordHash],
       [u].[PhoneNumber], [u].[PhoneNumberConfirmed], [u].[RoleId],
       [u].[SecurityStamp], [u].[TwoFactorEnabled], [u].[UserName],
       [a].[Id], [a].[IsBlocked], [a].[Money], [a].[UserId],
       [r].[Id], [r].[ConcurrencyStamp], [r].[Description], [r].[Name], [r].[NormalizedName]
FROM [AspNetUsers] AS [u]
LEFT JOIN [Accounts] AS [a] ON [u].[Id] = [a].[UserId]
INNER JOIN [AspNetRoles] AS [r] ON [u].[RoleId] = [r].[Id]
WHERE [u].[Id] = @__userId_0
```

### Resultado real (ejecutado contra DigitalArsDb)

```
Id  FirstName  LastName    Email                 Saldo      Rol
--  ---------  --------    -----                 -----      ---
1   Admin      DigitalArs  admin@digitalars.com  500000.00  Admin
```

### Analisis

- **Indices utilizados:** PK de `AspNetUsers`, `IX_Accounts_UserId` (unique), PK de `AspNetRoles`.
- **Plan de ejecucion esperado:** 3 Index Seeks (uno por tabla). Costo muy bajo.
- El `LEFT JOIN` con Accounts es correcto porque un usuario podria existir sin cuenta (aunque el negocio no lo permite, EF Core genera LEFT por seguridad con navegaciones opcionales).
- El `INNER JOIN` con Roles es porque `RoleId` es NOT NULL.

---

## Consulta 2: Historial de transacciones de una cuenta por rango de fechas

**Caso de uso:** Mostrar los movimientos de una cuenta entre dos fechas, ordenados del mas reciente al mas antiguo.

### LINQ

```csharp
var transacciones = await _context.Transactions
    .Where(t => t.AccountId == accountId
             && t.Date >= fechaDesde
             && t.Date <= fechaHasta)
    .OrderByDescending(t => t.Date)
    .ToListAsync();
```

### SQL generado

```sql
SELECT [t].[Id], [t].[AccountId], [t].[Amount], [t].[Concept],
       [t].[Date], [t].[ToAccountId], [t].[Type]
FROM [Transactions] AS [t]
WHERE [t].[AccountId] = @__accountId_0
  AND [t].[Date] >= @__fechaDesde_1
  AND [t].[Date] <= @__fechaHasta_2
ORDER BY [t].[Date] DESC
```

### Resultado real (ejecutado contra DigitalArsDb)

```
(0 filas — no hay transacciones registradas aun en la BD seed)
```

> Nota: La tabla Transactions esta vacia en el seed inicial. Los resultados apareceran cuando se implementen los endpoints de deposito y transferencia en Etapa 2.

### Analisis

- **Indices utilizados:** `IX_Transactions_AccountId` para filtrar por cuenta, `IX_Transactions_Date` para el rango y el ordenamiento.
- SQL Server puede hacer un Index Seek en `AccountId` y luego filtrar por `Date`, o viceversa dependiendo de la selectividad.
- **Optimizacion potencial:** Un indice compuesto `(AccountId, Date)` seria ideal si esta consulta tiene mucho volumen. Ver reporte de optimizacion.

---

## Consulta 3: Transferencias recibidas por una cuenta (TransferIn)

**Caso de uso:** Listar todas las transferencias donde la cuenta fue destino, incluyendo datos de la cuenta origen.

### LINQ

```csharp
var transferenciasRecibidas = await _context.Transactions
    .Include(t => t.Account)
        .ThenInclude(a => a!.User)
    .Where(t => t.ToAccountId == accountId
             && t.Type == TransactionType.TransferIn)
    .OrderByDescending(t => t.Date)
    .Select(t => new
    {
        t.Id,
        t.Amount,
        t.Concept,
        t.Date,
        OrigenNombre = t.Account!.User!.FirstName + " " + t.Account.User.LastName
    })
    .ToListAsync();
```

### SQL generado

```sql
SELECT [t].[Id], [t].[Amount], [t].[Concept], [t].[Date],
       [u].[FirstName] + N' ' + [u].[LastName] AS [OrigenNombre]
FROM [Transactions] AS [t]
INNER JOIN [Accounts] AS [a] ON [t].[AccountId] = [a].[Id]
INNER JOIN [AspNetUsers] AS [u] ON [a].[UserId] = [u].[Id]
WHERE [t].[ToAccountId] = @__accountId_0
  AND [t].[Type] = 2
ORDER BY [t].[Date] DESC
```

### Resultado real (ejecutado contra DigitalArsDb)

```
(0 filas — no hay transacciones registradas aun en la BD seed)
```

### Analisis

- **Indices utilizados:** `IX_Transactions_ToAccountId` para filtrar destino, PK de `Accounts`, `IX_Accounts_UserId` para resolver el JOIN con User.
- EF Core es inteligente: aunque usamos `Include`, al hacer `Select` con proyeccion, ignora el Include y solo trae las columnas necesarias.
- El filtro por `Type = 2` (TransferIn) se combina con el filtro de `ToAccountId` para reducir el result set.
- **INNER JOIN** es correcto aca: toda transaccion tiene una cuenta origen (AccountId NOT NULL).

---

## Consulta 4: Resumen de saldos por rol (consulta administrativa)

**Caso de uso:** El administrador quiere ver un resumen agrupado: cuantos usuarios hay por rol y el saldo total/promedio de sus cuentas.

### LINQ

```csharp
var resumen = await _context.Users
    .Where(u => !u.IsDeleted)
    .Include(u => u.Account)
    .Include(u => u.Role)
    .GroupBy(u => u.Role!.Name)
    .Select(g => new
    {
        Rol = g.Key,
        CantidadUsuarios = g.Count(),
        SaldoTotal = g.Sum(u => u.Account!.Money),
        SaldoPromedio = g.Average(u => u.Account!.Money)
    })
    .ToListAsync();
```

### SQL generado

```sql
SELECT [r].[Name] AS [Rol],
       COUNT(*) AS [CantidadUsuarios],
       COALESCE(SUM([a].[Money]), 0.0) AS [SaldoTotal],
       AVG([a].[Money]) AS [SaldoPromedio]
FROM [AspNetUsers] AS [u]
INNER JOIN [AspNetRoles] AS [r] ON [u].[RoleId] = [r].[Id]
LEFT JOIN [Accounts] AS [a] ON [u].[Id] = [a].[UserId]
WHERE [u].[IsDeleted] = CAST(0 AS bit)
GROUP BY [r].[Name]
```

### Resultado real (ejecutado contra DigitalArsDb)

```
Rol    CantidadUsuarios  SaldoTotal  SaldoPromedio
---    ----------------  ----------  -------------
Admin  1                 500000.00   500000.000000
User   2                 445000.50   222500.250000
```

### Analisis

- **Indices utilizados:** FK index en `RoleId`, `IX_Accounts_UserId` para el JOIN.
- `COALESCE` previene NULLs en la suma cuando un usuario no tiene cuenta (LEFT JOIN).
- Esta consulta es de baja frecuencia (solo admin), por lo que no justifica indices adicionales.
- Con 3 usuarios seed el costo es irrelevante; en produccion con miles de usuarios, se podria materializar como vista si se ejecuta frecuentemente.

---

## Datos completos de la BD (referencia)

Todos los usuarios registrados en el sistema:

```
Id  FirstName  LastName    Email                   Saldo      Rol
--  ---------  --------    -----                   -----      ---
1   Admin      DigitalArs  admin@digitalars.com    500000.00  Admin
2   Roberto    Carlos      robercarlos3@gmail.com  260000.00  User
3   Mohammed   Khan        mokha@gmail.com         185000.50  User
```

---

## Notas sobre la generacion del SQL

- El SQL mostrado es el que EF Core 10 genera con el provider `Microsoft.EntityFrameworkCore.SqlServer`.
- Se puede verificar en tiempo de ejecucion con:
  ```csharp
  var sql = query.ToQueryString(); // antes del await
  ```
- O habilitando logging sensible en desarrollo:
  ```csharp
  options.UseSqlServer(connectionString)
      .EnableSensitiveDataLogging()
      .LogTo(Console.WriteLine, LogLevel.Information);
  ```
