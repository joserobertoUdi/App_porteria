/*
  Diagnóstico de SQL Server, integridad y normalización.
  Solo lectura: no modifica datos ni esquema.
  Resultado esperado: las consultas de anomalías deben devolver cero filas.
*/
USE UniversidadDB;
GO
SET NOCOUNT ON;

/* 1. Confirma motor SQL Server, edición, compatibilidad y estado de la BD. */
SELECT SERVERPROPERTY('ProductVersion') AS VersionSqlServer,
       SERVERPROPERTY('ProductLevel') AS Nivel,
       SERVERPROPERTY('Edition') AS Edicion,
       SERVERPROPERTY('EngineEdition') AS Motor,
       DATABASEPROPERTYEX(DB_NAME(), 'Status') AS EstadoBase,
       DATABASEPROPERTYEX(DB_NAME(), 'CompatibilityLevel') AS NivelCompatibilidad;

/* 2. Migraciones EF aplicadas. Debe incluir la última migración del repositorio. */
SELECT MigrationId, ProductVersion
FROM dbo.__EFMigrationsHistory
ORDER BY MigrationId;

/* 3. Relaciones físicas (la existencia de FKs es esencial para 3FN). */
SELECT fk.name AS ClaveForanea,
       OBJECT_SCHEMA_NAME(fk.parent_object_id) + N'.' + OBJECT_NAME(fk.parent_object_id) AS TablaHija,
       OBJECT_SCHEMA_NAME(fk.referenced_object_id) + N'.' + OBJECT_NAME(fk.referenced_object_id) AS TablaPadre,
       fk.delete_referential_action_desc AS Borrado,
       fk.is_disabled AS EstaDeshabilitada,
       fk.is_not_trusted AS NoValidada
FROM sys.foreign_keys fk
WHERE OBJECT_SCHEMA_NAME(fk.parent_object_id) = N'dbo'
ORDER BY TablaHija, ClaveForanea;

/* 4. Índices únicos que previenen duplicados de identidad de negocio. */
SELECT OBJECT_NAME(i.object_id) AS Tabla, i.name AS Indice, i.is_unique AS EsUnico,
       c.name AS Columna, ic.key_ordinal AS OrdenColumna
FROM sys.indexes i
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE i.is_unique = 1
  AND OBJECT_SCHEMA_NAME(i.object_id) = N'dbo'
ORDER BY Tabla, Indice, OrdenColumna;

/* 5. Huérfanos: esperado cero filas en cada consulta. */
SELECT N'Usuarios sin TipoUsuario' AS Regla, u.Id AS IdProblema
FROM dbo.Usuarios u LEFT JOIN dbo.TiposUsuario t ON t.Id = u.TipoUsuarioId WHERE t.Id IS NULL
UNION ALL
SELECT N'Usuarios con Rol inexistente', u.Id
FROM dbo.Usuarios u LEFT JOIN dbo.Roles r ON r.Id = u.RolId WHERE u.RolId IS NOT NULL AND r.Id IS NULL
UNION ALL
SELECT N'Vehiculos sin Usuario', v.Id
FROM dbo.Vehiculos v LEFT JOIN dbo.Usuarios u ON u.Id = v.UsuarioId WHERE u.Id IS NULL
UNION ALL
SELECT N'RegistrosPorteria sin Usuario', rp.Id
FROM dbo.RegistrosPorteria rp LEFT JOIN dbo.Usuarios u ON u.Id = rp.UsuarioId WHERE u.Id IS NULL
UNION ALL
SELECT N'RegistrosParqueo sin Vehiculo', rp.Id
FROM dbo.RegistrosParqueo rp LEFT JOIN dbo.Vehiculos v ON v.Id = rp.VehiculoId WHERE v.Id IS NULL
UNION ALL
SELECT N'RefreshTokens sin Usuario', rt.Id
FROM dbo.RefreshTokens rt LEFT JOIN dbo.Usuarios u ON u.Id = rt.UsuarioId WHERE u.Id IS NULL;

/* 6. Duplicados de catálogos (recomendación: también deben ser únicos por nombre). */
SELECT N'TiposUsuario duplicados' AS Regla, Nombre, COUNT(*) AS Cantidad
FROM dbo.TiposUsuario GROUP BY Nombre HAVING COUNT(*) > 1
UNION ALL
SELECT N'Roles duplicados', Nombre, COUNT(*)
FROM dbo.Roles GROUP BY Nombre HAVING COUNT(*) > 1;

/* 7. Coherencia temporal. Esperado: cero filas. */
SELECT N'Porteria: salida anterior a entrada' AS Regla, Id
FROM dbo.RegistrosPorteria WHERE FechaSalida IS NOT NULL AND FechaSalida < FechaEntrada
UNION ALL
SELECT N'Parqueo: salida anterior a ingreso', Id
FROM dbo.RegistrosParqueo WHERE FechaSalida IS NOT NULL AND FechaSalida < FechaIngreso
UNION ALL
SELECT N'Refresh token: expira antes de crearse', Id
FROM dbo.RefreshTokens WHERE FechaExpiracion <= FechaCreacion
UNION ALL
SELECT N'Refresh token: se revocó antes de crearse', Id
FROM dbo.RefreshTokens WHERE FechaRevocacion IS NOT NULL AND FechaRevocacion < FechaCreacion;

/* 8. Auditoría instalada: las siete tablas deben tener Ride y FechaModificacion. */
SELECT t.name AS Tabla,
       MAX(CASE WHEN c.name = N'Ride' THEN 1 ELSE 0 END) AS TieneRide,
       MAX(CASE WHEN c.name = N'FechaModificacion' THEN 1 ELSE 0 END) AS TieneFechaModificacion,
       MAX(CASE WHEN c.name = N'FechaRegistro' THEN 1 ELSE 0 END) AS TieneFechaRegistro
FROM sys.tables t
LEFT JOIN sys.columns c ON c.object_id = t.object_id
WHERE t.schema_id = SCHEMA_ID(N'dbo')
  AND t.name IN (N'TiposUsuario', N'Roles', N'Usuarios', N'Vehiculos', N'RegistrosPorteria', N'RegistrosParqueo', N'RefreshTokens')
GROUP BY t.name
ORDER BY t.name;

/* 9. Consistencia de Ride. Esperado: cero filas. */
SELECT N'Usuarios' AS Tabla, COUNT(*) AS RidesNulos FROM dbo.Usuarios WHERE Ride IS NULL
UNION ALL SELECT N'Vehiculos', COUNT(*) FROM dbo.Vehiculos WHERE Ride IS NULL
UNION ALL SELECT N'RegistrosPorteria', COUNT(*) FROM dbo.RegistrosPorteria WHERE Ride IS NULL
UNION ALL SELECT N'RegistrosParqueo', COUNT(*) FROM dbo.RegistrosParqueo WHERE Ride IS NULL
UNION ALL SELECT N'RefreshTokens', COUNT(*) FROM dbo.RefreshTokens WHERE Ride IS NULL;
GO
