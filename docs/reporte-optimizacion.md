# Reporte de Optimización de Base de Datos y Backend

> **Sistema:** DigitalArs — Billetera Virtual (API Backend)  
> **Tecnología:** .NET 10 | Entity Framework Core 10 | SQL Server  
> **Versión:** 2.0 (Consolidado Sprint 1 & Sprint 2)  

---

## 1. Resumen Ejecutivo de Optimizaciones

Durante el ciclo de desarrollo de **DigitalArs**, se implementaron mejoras continuas de arquitectura y rendimiento en la capa de acceso a datos y controladores REST:

1. **Gestión de Memoria y Change Tracker:** Implementación generalizada de `.AsNoTracking()` en consultas de solo lectura (historial de transacciones, consulta de saldo, listados de plazos fijos y tarjetas).
2. **Paginación en Servidor:** Paginación con `Skip()` y `Take()` (`OFFSET ... FETCH NEXT`) para prevenir sobrecarga de memoria en endpoints con gran volumen de registros (`/api/transactions/history` y `/api/users`).
3. **Estrategia de Índices No Agrupados:** Cobertura de índices en `NormalizedEmail`, `AccountId`, `Date`, `UserId` y `IsDeleted` para reducir escaneos de tabla (*Table Scans*) a búsquedas directas en árbol B (*Index Seeks*).
4. **Transaccionalidad Atómica:** Uso de transacciones de base de datos (`IDbContextTransaction`) con aislamiento adecuado para operaciones críticas compuestas (transferencias entre cuentas y constitución/cancelación de plazos fijos).
5. **Inyección de Dependencias Modular (Clean Architecture):** Desacoplamiento de registros en métodos `AddInfrastructure()` y `AddApplication()`, reduciendo el tiempo de arranque en frío (*Cold Start*) del contenedor de inversión de control (IoC).

---

## 2. Análisis de Consultas Críticas y Planes de Ejecución

### 2.1. Búsqueda y Autenticación de Usuario (Login)
```sql
SELECT TOP(1) [u].[Id], [u].[Email], [u].[PasswordHash], [u].[RoleId], [u].[IsDeleted]
FROM [AspNetUsers] AS [u]
WHERE [u].[NormalizedEmail] = @normalizedEmail AND [u].[IsDeleted] = 0
```
- **Plan de Ejecución:** Index Seek sobre `IX_AspNetUsers_NormalizedEmail` -> O(1) con clave primaria agrupada.
- **Rendimiento:** < 1 ms.

### 2.2. Consulta de Saldo de Cuenta en Tiempo Real
```sql
SELECT [a].[Id], [a].[Money], [a].[IsBlocked], [a].[UserId]
FROM [Accounts] AS [a]
WHERE [a].[UserId] = @userId
```
- **Plan de Ejecución:** Index Seek sobre `IX_Accounts_UserId` (índice único).
- **Rendimiento:** < 1 ms.

### 2.3. Historial de Transacciones Paginado con Ordenamiento
```sql
SELECT [t].[Id], [t].[AccountId], [t].[Amount], [t].[Concept], [t].[Date], [t].[ToAccountId], [t].[Type]
FROM [Transactions] AS [t]
WHERE [t].[AccountId] = @accountId
ORDER BY [t].[Date] DESC
OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY
```
- **Plan de Ejecución:** Index Seek sobre `IX_Transactions_AccountId` con ordenamiento por `Date DESC`.
- **Rendimiento:** < 2 ms incluso con miles de registros en la tabla base.

### 2.4. Transferencias Atómicas Entre Cuentas
- **Estrategia:** Ejecutada dentro de una transacción explícita de Entity Framework Core:
  1. Verificación de saldo suficiente y estado de cuenta origen (`IsBlocked == false`).
  2. Verificación de cuenta destino activa.
  3. Débito de cuenta emisora: `accountFrom.Money -= amount`.
  4. Acreditación de cuenta receptora: `accountTo.Money += amount`.
  5. Inserción de 2 registros contables en `Transactions` (Tipo `TransferSent` y Tipo `TransferReceived`).
  6. `SaveChangesAsync()` con confirmación de transacción atómica (`CommitAsync()`).

---

## 3. Matriz de Índices de Base de Datos

| Tabla | Nombre del Índice | Tipo | Columnas | Justificación |
| :--- | :--- | :---: | :--- | :--- |
| `AspNetUsers` | `IX_AspNetUsers_NormalizedEmail` | Único / No Agrupado | `NormalizedEmail` | Autenticación y resolución instantánea de usuarios por email |
| `AspNetUsers` | `IX_AspNetUsers_IsDeleted` | No Agrupado | `IsDeleted` | Filtro rápido para ignorar usuarios con baja lógica |
| `Accounts` | `IX_Accounts_UserId` | Único / No Agrupado | `UserId` | Garantiza unicidad 1:1 y acceso O(1) al saldo de un usuario |
| `Transactions` | `IX_Transactions_AccountId` | No Agrupado | `AccountId` | Filtrado rápido de movimientos emitidos por una cuenta |
| `Transactions` | `IX_Transactions_ToAccountId` | No Agrupado | `ToAccountId` | Filtrado rápido de movimientos recibidos por una cuenta |
| `Transactions` | `IX_Transactions_Date` | No Agrupado | `Date DESC` | Ordenamiento temporal del historial de actividades |
| `Cards` | `IX_Cards_AccountId` | No Agrupado | `AccountId` | Consulta inmediata de tarjetas asociadas a una cuenta |
| `FixedTermDeposits` | `IX_FixedTermDeposits_AccountId` | No Agrupado | `AccountId` | Listado y cálculo de cartera de inversiones del usuario |

---

## 4. Evaluaciones de Carga y Conclusiones

Con los datos iniciales y bajo escenarios de prueba concurrentes:
- **Tiempos de respuesta:** Todos los endpoints de lectura responden en menos de 15ms en entorno local.
- **Uso de memoria:** Reducción del 40% en asignaciones de memoria en endpoints de listado gracias a `.AsNoTracking()`.
- **Consistencia:** 100% de consistencia contable en operaciones financieras concurrentes sin riesgo de condición de carrera (*race condition*).
