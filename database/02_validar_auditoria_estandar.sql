/* Ejecutar después de 01_auditoria_estandar.sql en UniversidadDB. No modifica datos. */
USE UniversidadDB;
GO

SET NOCOUNT ON;

/* 1. Todas las tablas de aplicación tienen Ride y FechaModificacion. */
SELECT t.name AS Tabla,
       MAX(CASE WHEN c.name = 'Ride' THEN 1 ELSE 0 END) AS TieneRide,
       MAX(CASE WHEN c.name = 'FechaModificacion' THEN 1 ELSE 0 END) AS TieneFechaModificacion
FROM sys.tables t
LEFT JOIN sys.columns c ON c.object_id = t.object_id
WHERE t.name IN ('TiposUsuario','Roles','Usuarios','Vehiculos','RegistrosPorteria','RegistrosParqueo','RefreshTokens')
GROUP BY t.name
ORDER BY t.name;

/* 2. No debe devolver filas: Ride nulo o duplicado. */
SELECT 'Usuarios' AS Tabla, Ride, COUNT(*) AS Cantidad FROM dbo.Usuarios GROUP BY Ride HAVING Ride IS NULL OR COUNT(*) > 1
UNION ALL SELECT 'Vehiculos', Ride, COUNT(*) FROM dbo.Vehiculos GROUP BY Ride HAVING Ride IS NULL OR COUNT(*) > 1
UNION ALL SELECT 'RegistrosPorteria', Ride, COUNT(*) FROM dbo.RegistrosPorteria GROUP BY Ride HAVING Ride IS NULL OR COUNT(*) > 1
UNION ALL SELECT 'RegistrosParqueo', Ride, COUNT(*) FROM dbo.RegistrosParqueo GROUP BY Ride HAVING Ride IS NULL OR COUNT(*) > 1;

/* 3. Triggers de auditoría instalados y habilitados. Debe devolver siete filas. */
SELECT name AS Trigger, OBJECT_NAME(parent_id) AS Tabla, is_disabled AS EstaDeshabilitado
FROM sys.triggers
WHERE name LIKE 'TR[_]%[_]Auditoria'
ORDER BY Tabla;

/* 4. Últimos movimientos. PasswordHash y TokenHash no deben aparecer. */
SELECT TOP (100) AuditoriaId, Tabla, RegistroId, RegistroRide, Operacion, FechaEvento, UsuarioSql, Aplicacion, Equipo, DatosAntes, DatosDespues
FROM dbo.AuditoriaMovimientos
ORDER BY AuditoriaId DESC;
GO
