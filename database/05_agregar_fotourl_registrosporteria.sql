/* ============================================================================
   Portería — Foto del registro de entrada

   Agrega la columna que guarda la referencia a la foto en el servicio
   documental (SharepointApi). No se guarda la imagen ni una URL absoluta,
   solo el identificador en formato 'sharepoint:{uid}' — así, si cambia el
   host o la ruta del servicio, los registros históricos siguen resolviendo.

   Vía recomendada: generar la migración desde el modelo, para que el snapshot
   de EF Core quede sincronizado.

       cd ServiciosGenerales\ServiciosGenerales
       dotnet ef migrations add AddFotoUrlRegistroPorteria ^
              --project ServiciosGenerales.Infraestructura ^
              --startup-project ServiciosGenerales.Api
       dotnet ef database update ^
              --project ServiciosGenerales.Infraestructura ^
              --startup-project ServiciosGenerales.Api

   Este script existe solo como alternativa manual. Si se aplica por aquí, el
   snapshot de EF Core queda desfasado y la próxima migración intentará crear
   la columna otra vez.

   Idempotente: se puede ejecutar varias veces sin efecto adicional.
   ============================================================================ */

IF NOT EXISTS (
    SELECT 1
      FROM sys.columns
     WHERE object_id = OBJECT_ID('dbo.RegistrosPorteria')
       AND name = 'FotoUrl'
)
BEGIN
    ALTER TABLE dbo.RegistrosPorteria
        ADD FotoUrl NVARCHAR(200) NULL;

    PRINT 'Columna FotoUrl agregada a dbo.RegistrosPorteria.';
END
ELSE
BEGIN
    PRINT 'La columna FotoUrl ya existía. Sin cambios.';
END
GO

/* Comprobación */
SELECT c.name AS Columna, t.name AS Tipo, c.max_length / 2 AS Longitud, c.is_nullable AS Nulable
  FROM sys.columns c
  JOIN sys.types t ON t.user_type_id = c.user_type_id
 WHERE c.object_id = OBJECT_ID('dbo.RegistrosPorteria')
   AND c.name = 'FotoUrl';
GO
