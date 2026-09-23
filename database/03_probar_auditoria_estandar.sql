/*
  Prueba no persistente de la auditoría.
  Ejecutar únicamente después de 01_auditoria_estandar.sql.
  Inserta, actualiza y elimina un tipo temporal; ROLLBACK revierte datos y auditoría.
*/
USE UniversidadDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @NombrePrueba NVARCHAR(50) = N'__PRUEBA_AUDITORIA_' + CONVERT(NVARCHAR(36), NEWID());
DECLARE @IdPrueba INT;

INSERT dbo.TiposUsuario (Nombre) VALUES (@NombrePrueba);
SET @IdPrueba = SCOPE_IDENTITY();

UPDATE dbo.TiposUsuario
SET Nombre = @NombrePrueba + N'_ACTUALIZADO'
WHERE Id = @IdPrueba;

DELETE FROM dbo.TiposUsuario WHERE Id = @IdPrueba;

/* Deben verse exactamente INSERT, UPDATE y DELETE antes del rollback. */
SELECT Operacion, COUNT(*) AS Cantidad
FROM dbo.AuditoriaMovimientos
WHERE Tabla = N'TiposUsuario'
  AND (DatosAntes LIKE N'%' + @NombrePrueba + N'%' OR DatosDespues LIKE N'%' + @NombrePrueba + N'%')
GROUP BY Operacion
ORDER BY Operacion;

IF (SELECT COUNT(*)
    FROM dbo.AuditoriaMovimientos
    WHERE Tabla = N'TiposUsuario'
      AND (DatosAntes LIKE N'%' + @NombrePrueba + N'%' OR DatosDespues LIKE N'%' + @NombrePrueba + N'%')) <> 3
    THROW 51000, 'Falló la prueba: se esperaban tres eventos de auditoría.', 1;

ROLLBACK TRANSACTION;
PRINT 'Prueba correcta: la transacción fue revertida y no dejó datos.';
GO
