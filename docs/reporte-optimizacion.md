# Reporte de Optimizacion - Etapa 1

## Resumen ejecutivo

Este reporte evalua el modelo de datos de DigitalArs al cierre de la Etapa 1, analiza el rendimiento esperado de las consultas principales, e identifica oportunidades de mejora para cuando el sistema escale.

---

## 1. Evaluacion de indices actuales

### Indices implementados

| Indice                      | Tabla        | Tipo                            | Columnas           | Justificado   |
| --------------------------- | ------------ | ------------------------------- | ------------------ | ------------- |
| PK_AspNetUsers              | AspNetUsers  | Clustered                       | Id                 | Si (PK)       |
| IX_AspNetUsers_Email        | AspNetUsers  | Non-clustered, Unique, Filtered | Email              | Si            |
| UserNameIndex               | AspNetUsers  | Non-clustered, Unique           | NormalizedUserName | Si (Identity) |
| EmailIndex                  | AspNetUsers  | Non-clustered                   | NormalizedEmail    | Si (Identity) |
| PK_Accounts                 | Accounts     | Clustered                       | Id                 | Si (PK)       |
| IX_Accounts_UserId          | Accounts     | Non-clustered, Unique           | UserId             | Si            |
| PK_Transactions             | Transactions | Clustered                       | Id                 | Si (PK)       |
| IX_Transactions_Date        | Transactions | Non-clustered                   | Date               | Si            |
| IX_Transactions_AccountId   | Transactions | Non-clustered                   | AccountId          | Si (FK)       |
| IX_Transactions_ToAccountId | Transactions | Non-clustered                   | ToAccountId        | Si (FK)       |

### Conclusion: No hay indices innecesarios

Todos los indices cumplen una funcion concreta. No se encontro redundancia ni indices duplicados.

---

## 2. Analisis de planes de ejecucion

### Consulta 1: Login / Busqueda por email

```sql
SELECT TOP(1) [u].[Id], [u].[Email], [u].[PasswordHash], ... FROM [AspNetUsers] AS [u] WHERE [u].[Email] = @email
```

**Plan esperado:**

- Index Seek en `IX_AspNetUsers_Email` → Key Lookup en PK (clustered)
- **Costo estimado:** Muy bajo. O(1) con el indice unico.
- **Riesgo a escala:** Ninguno. Un indice unico siempre resuelve en una sola pagina.

### Consulta 2: Historial de transacciones por cuenta + rango de fechas

```sql
SELECT [t].[Id], [t].[AccountId], [t].[Amount], [t].[Date], ... FROM [Transactions] AS [t]
WHERE [t].[AccountId] = @id AND [t].[Date] BETWEEN @desde AND @hasta
ORDER BY [t].[Date] DESC
```

**Plan esperado:**

- Index Seek en `IX_Transactions_AccountId` + filtro residual por `Date`
- O: Index Seek en `IX_Transactions_Date` + filtro residual por `AccountId`
- SQL Server elige segun estadisticas (selectividad de cada filtro).

**Riesgo a escala:** Con millones de transacciones, ningun indice individual cubre ambos filtros eficientemente. Ver recomendacion en seccion 4.

### Consulta 3: Saldo de cuenta por usuario

```sql
SELECT [a].[Id], [a].[IsBlocked], [a].[Money], [a].[UserId] FROM [Accounts] AS [a] WHERE [a].[UserId] = @userId
```

**Plan esperado:**

- Index Seek en `IX_Accounts_UserId` (unico) → 1 resultado garantizado.
- **Riesgo a escala:** Ninguno. Operacion constante O(1).

### Consulta 4: Transferencias recibidas con datos del origen

```sql
SELECT ... FROM [Transactions] AS [t]
INNER JOIN [Accounts] AS [a] ON [t].[AccountId] = [a].[Id]
INNER JOIN [AspNetUsers] AS [u] ON [a].[UserId] = [u].[Id]
WHERE [t].[ToAccountId] = @id AND [t].[Type] = 2
ORDER BY [t].[Date] DESC
```

**Plan esperado:**

- Seek en `IX_Transactions_ToAccountId` → Nested Loop Join con Accounts (PK) → Nested Loop Join con Users (PK).
- Filtro por `Type = 2` se aplica como predicado residual.

**Riesgo a escala:** Si una cuenta recibe miles de transferencias, el filtro residual por Type recorre muchas filas. Ver recomendacion en seccion 4.

---

## 3. Evaluacion de decisiones de diseño

### 3.1 Rendimiento general

| Aspecto                  | Estado   | Nota                              |
| ------------------------ | -------- | --------------------------------- |
| Tipo de PK (int vs GUID) | Optimo   | 4 bytes, clustered index compacto |
| Precision decimal(18,2)  | Correcto | Sin riesgo de overflow ni perdida |
| Relaciones con Restrict  | Correcto | Evita ciclos, fuerza soft-delete  |
| Enum como int            | Optimo   | 4 bytes, comparaciones rapidas    |
| Indice en Date           | Correcto | Cubre la query mas frecuente      |

### 3.2 Posibles problemas detectados

| #   | Problema                                                    | Impacto                                                                             | Prioridad                              |
| --- | ----------------------------------------------------------- | ----------------------------------------------------------------------------------- | -------------------------------------- |
| 1   | No hay indice compuesto (AccountId, Date) en Transactions   | Consultas de historial por cuenta con rango de fechas no estan totalmente cubiertas | Media (cuando haya volumen)            |
| 2   | No hay indice en User.IsDeleted                             | Filtro `WHERE IsDeleted = 0` no tiene soporte de indice                             | Baja (tabla chica por ahora)           |
| 3   | No hay indice compuesto (ToAccountId, Type) en Transactions | Consultas de transferencias recibidas filtradas por tipo                            | Baja                                   |
| 4   | Uso de Include() en vez de proyecciones                     | EF Core trae todas las columnas de la entidad aunque no se usen todas               | Baja (mitigar con proyecciones Select) |

---

## 4. Recomendaciones para Etapa 2

### 4.1 Indice compuesto para historial de transacciones (Prioridad MEDIA)

```sql
CREATE NONCLUSTERED INDEX IX_Transactions_AccountId_Date
ON [Transactions] ([AccountId], [Date] DESC)
INCLUDE ([Amount], [Type], [Concept], [ToAccountId]);
```

**Beneficio:** Cubre completamente la consulta de historial sin Key Lookup. El `INCLUDE` evita ir a la tabla base.

**Cuando aplicar:** Cuando la tabla Transactions supere ~10.000 filas y los tiempos de respuesta del historial aumenten.

**En Fluent API:**

```csharp
builder.HasIndex(t => new { t.AccountId, t.Date })
    .IsDescending(false, true)
    .HasDatabaseName("IX_Transactions_AccountId_Date");
```

### 4.2 Paginacion en consultas de historial (Prioridad MEDIA)

Actualmente las consultas traen TODAS las transacciones. Con volumen esto es insostenible.

**Recomendacion:**

```csharp
var transacciones = await _context.Transactions
    .Where(t => t.AccountId == accountId)
    .OrderByDescending(t => t.Date)
    .Skip(page * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

EF Core genera `OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY`, que con el indice compuesto propuesto es extremadamente eficiente.

### 4.3 Proyecciones en vez de Include (Prioridad BAJA)

En vez de cargar entidades completas con `Include()`, usar `Select()` para traer solo las columnas necesarias:

```csharp
// Menos eficiente (trae todas las columnas)
var user = await _context.Users.Include(u => u.Account).FirstAsync(u => u.Id == id);

// Mas eficiente (solo lo que se necesita)
var perfil = await _context.Users
    .Where(u => u.Id == id)
    .Select(u => new { u.FirstName, u.LastName, u.Email, Saldo = u.Account!.Money })
    .FirstAsync();
```

**SQL resultante mas liviano:**

```sql
SELECT [u].[FirstName], [u].[LastName], [u].[Email], [a].[Money] AS [Saldo]
FROM [AspNetUsers] AS [u]
LEFT JOIN [Accounts] AS [a] ON [u].[Id] = [a].[UserId]
WHERE [u].[Id] = @__id_0
```

### 4.4 Considerar AsNoTracking para consultas de solo lectura (Prioridad BAJA)

```csharp
var transacciones = await _context.Transactions
    .AsNoTracking()
    .Where(t => t.AccountId == accountId)
    .ToListAsync();
```

**Beneficio:** EF Core no guarda las entidades en el Change Tracker, reduciendo uso de memoria y CPU. Util para endpoints de lectura como historial y reportes.

---

## 5. Metricas de referencia (baseline con seed data)

Con los datos iniciales (3 usuarios, 3 cuentas, 0 transacciones):

| Consulta                   | Filas leidas | Tiempo estimado | Indice usado                     |
| -------------------------- | ------------ | --------------- | -------------------------------- |
| Buscar usuario por email   | 1            | <1ms            | IX_AspNetUsers_Email (Seek)      |
| Obtener cuenta por userId  | 1            | <1ms            | IX_Accounts_UserId (Seek)        |
| Historial de transacciones | 0            | <1ms            | IX_Transactions_AccountId (Seek) |
| Resumen por rol (GROUP BY) | 3            | <1ms            | Full scan (tabla chica)          |

Estos valores sirven como baseline. Se recomienda re-evaluar cuando:

- La tabla Transactions supere 10.000 filas
- Los tiempos de respuesta de la API superen 100ms
- Se agreguen nuevos filtros o reportes complejos

---

## 6. Conclusion

El modelo de datos de la Etapa 1 esta **bien optimizado para su escala actual**. Los indices cubren las operaciones criticas (login, consulta de saldo, historial basico) y las decisiones de diseño (int PKs, decimal para montos, Restrict delete) son correctas.

Las optimizaciones pendientes (indice compuesto, paginacion, proyecciones) son mejoras para la Etapa 2 cuando el volumen de datos crezca. No hay problemas de rendimiento bloqueantes en esta etapa.

**Prioridades para Etapa 2:**

1. Implementar paginacion en endpoints de historial
2. Agregar indice compuesto `(AccountId, Date DESC)` cuando haya volumen
3. Usar `AsNoTracking()` en consultas de solo lectura
4. Preferir `Select()` sobre `Include()` en endpoints de API
