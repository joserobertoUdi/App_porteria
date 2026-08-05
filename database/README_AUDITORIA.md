# Auditoría estándar de base de datos

Esta implementación no modifica la API .NET, el panel Angular ni la aplicación Flutter. Está formada por:

- `01_auditoria_estandar.sql`: instalación idempotente de columnas, índices, auditoría y triggers.
- `02_validar_auditoria_estandar.sql`: consultas de validación sin cambios de datos.
- `03_probar_auditoria_estandar.sql`: prueba transaccional de INSERT, UPDATE y DELETE; siempre finaliza con `ROLLBACK`.
- `04_validar_integridad_y_normalizacion.sql`: diagnóstico de motor SQL Server, migraciones, claves foráneas, datos huérfanos, duplicados y cronología.

## Semántica del estándar

| Campo | Uso |
|---|---|
| `Ride` | GUID inmutable y único por fila. No sustituye la clave primaria `Id`. |
| `FechaRegistro` | Fecha de alta. Se conserva `FechaRegistro` de usuarios/vehículos, `FechaEntrada`/`FechaIngreso` para los movimientos y `FechaCreacion` para refresh tokens. |
| `FechaModificacion` | Última escritura en UTC, establecida por los triggers. |
| `Estado` | No se duplica: usuarios ya lo tienen; vehículos lo derivan de `Activo`; entradas/salidas se derivan de `FechaSalida`; tokens se derivan de si fueron revocados. |

`AuditoriaMovimientos` registra los valores anterior y nuevo, la operación, hora UTC, usuario técnico SQL, aplicación cliente y equipo. Por seguridad no guarda `PasswordHash` ni `TokenHash`.

## Instalación

1. Haga un respaldo de `UniversidadDB`.
2. Abra SSMS contra la base objetivo.
3. Ejecute `01_auditoria_estandar.sql` completo.
4. Ejecute `02_validar_auditoria_estandar.sql`.
5. Ejecute `03_probar_auditoria_estandar.sql`; debe devolver una fila para cada operación (`INSERT`, `UPDATE`, `DELETE`) y confirmar el rollback.
6. Ejecute desde la aplicación una alta y una actualización (por ejemplo entrada/salida) y vuelva a correr la consulta 4 de validación.
7. Ejecute `04_validar_integridad_y_normalizacion.sql`. Las secciones de anomalías (huérfanos, duplicados y fechas inválidas) deben devolver cero filas.

La API no conoce el usuario final dentro de SQL Server; por eso `UsuarioSql` identifica la cuenta técnica de conexión. Para auditar el administrador o portero real se requeriría una mejora posterior en API que establezca `SESSION_CONTEXT` por solicitud.
