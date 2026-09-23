/*
  Auditoría de movimientos - Sistema de Estacionamiento UDI

  Alcance: solo SQL Server. No requiere cambios en API, panel Angular ni app Flutter.
  Requisitos: SQL Server 2016 SP1 o posterior (CREATE OR ALTER TRIGGER).

  Estándar aplicado
  - Ride: identificador global e inmutable de cada fila.
  - FechaRegistro: fecha de creación. Se reutilizan las fechas de negocio ya existentes.
  - FechaModificacion: última modificación, controlada por SQL Server en UTC.
  - Estado: se reutiliza o deriva del estado de negocio para no duplicar información.
  - AuditoriaMovimientos: historial append-only de INSERT, UPDATE y DELETE.

  Antes de ejecutar: realizar un backup de UniversidadDB.
*/
USE UniversidadDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT OFF;
GO

/* ----- Tabla histórica, independiente de EF Core ----- */
IF OBJECT_ID(N'dbo.AuditoriaMovimientos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditoriaMovimientos
    (
        AuditoriaId       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditoriaMovimientos PRIMARY KEY,
        Ride               UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AuditoriaMovimientos_Ride DEFAULT NEWSEQUENTIALID(),
        Tabla              SYSNAME NOT NULL,
        RegistroId         INT NULL,
        RegistroRide       UNIQUEIDENTIFIER NULL,
        Operacion          VARCHAR(10) NOT NULL,
        FechaEvento        DATETIME2(3) NOT NULL CONSTRAINT DF_AuditoriaMovimientos_FechaEvento DEFAULT SYSUTCDATETIME(),
        UsuarioSql         NVARCHAR(128) NOT NULL CONSTRAINT DF_AuditoriaMovimientos_UsuarioSql DEFAULT ORIGINAL_LOGIN(),
        Aplicacion         NVARCHAR(128) NULL CONSTRAINT DF_AuditoriaMovimientos_Aplicacion DEFAULT APP_NAME(),
        Equipo             NVARCHAR(128) NULL CONSTRAINT DF_AuditoriaMovimientos_Equipo DEFAULT HOST_NAME(),
        DatosAntes         NVARCHAR(MAX) NULL,
        DatosDespues       NVARCHAR(MAX) NULL,
        CONSTRAINT CK_AuditoriaMovimientos_Operacion CHECK (Operacion IN ('INSERT', 'UPDATE', 'DELETE'))
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.AuditoriaMovimientos') AND name = N'UX_AuditoriaMovimientos_Ride')
    CREATE UNIQUE INDEX UX_AuditoriaMovimientos_Ride ON dbo.AuditoriaMovimientos(Ride);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.AuditoriaMovimientos') AND name = N'IX_AuditoriaMovimientos_Registro')
    CREATE INDEX IX_AuditoriaMovimientos_Registro ON dbo.AuditoriaMovimientos(Tabla, RegistroId, FechaEvento DESC);

/* ----- Columnas estándar: Ride y FechaModificacion ----- */
IF COL_LENGTH(N'dbo.TiposUsuario', N'Ride') IS NULL
BEGIN
    ALTER TABLE dbo.TiposUsuario ADD Ride UNIQUEIDENTIFIER NULL;
    UPDATE dbo.TiposUsuario SET Ride = NEWID() WHERE Ride IS NULL;
    ALTER TABLE dbo.TiposUsuario ALTER COLUMN Ride UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE dbo.TiposUsuario ADD CONSTRAINT DF_TiposUsuario_Ride DEFAULT NEWSEQUENTIALID() FOR Ride;
END;

IF COL_LENGTH(N'dbo.TiposUsuario', N'FechaModificacion') IS NULL
BEGIN
    ALTER TABLE dbo.TiposUsuario ADD FechaModificacion DATETIME2(3) NULL;
    UPDATE dbo.TiposUsuario SET FechaModificacion = SYSUTCDATETIME() WHERE FechaModificacion IS NULL;
    ALTER TABLE dbo.TiposUsuario ALTER COLUMN FechaModificacion DATETIME2(3) NOT NULL;
    ALTER TABLE dbo.TiposUsuario ADD CONSTRAINT DF_TiposUsuario_FechaModificacion DEFAULT SYSUTCDATETIME() FOR FechaModificacion;
END;

IF COL_LENGTH(N'dbo.Roles', N'Ride') IS NULL
BEGIN
    ALTER TABLE dbo.Roles ADD Ride UNIQUEIDENTIFIER NULL;
    UPDATE dbo.Roles SET Ride = NEWID() WHERE Ride IS NULL;
    ALTER TABLE dbo.Roles ALTER COLUMN Ride UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE dbo.Roles ADD CONSTRAINT DF_Roles_Ride DEFAULT NEWSEQUENTIALID() FOR Ride;
END;

IF COL_LENGTH(N'dbo.Roles', N'FechaModificacion') IS NULL
BEGIN
    ALTER TABLE dbo.Roles ADD FechaModificacion DATETIME2(3) NULL;
    UPDATE dbo.Roles SET FechaModificacion = SYSUTCDATETIME() WHERE FechaModificacion IS NULL;
    ALTER TABLE dbo.Roles ALTER COLUMN FechaModificacion DATETIME2(3) NOT NULL;
    ALTER TABLE dbo.Roles ADD CONSTRAINT DF_Roles_FechaModificacion DEFAULT SYSUTCDATETIME() FOR FechaModificacion;
END;

IF COL_LENGTH(N'dbo.Usuarios', N'Ride') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios ADD Ride UNIQUEIDENTIFIER NULL;
    UPDATE dbo.Usuarios SET Ride = NEWID() WHERE Ride IS NULL;
    ALTER TABLE dbo.Usuarios ALTER COLUMN Ride UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE dbo.Usuarios ADD CONSTRAINT DF_Usuarios_Ride DEFAULT NEWSEQUENTIALID() FOR Ride;
END;

IF COL_LENGTH(N'dbo.Usuarios', N'FechaModificacion') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios ADD FechaModificacion DATETIME2(3) NULL;
    UPDATE dbo.Usuarios SET FechaModificacion = SYSUTCDATETIME() WHERE FechaModificacion IS NULL;
    ALTER TABLE dbo.Usuarios ALTER COLUMN FechaModificacion DATETIME2(3) NOT NULL;
    ALTER TABLE dbo.Usuarios ADD CONSTRAINT DF_Usuarios_FechaModificacion DEFAULT SYSUTCDATETIME() FOR FechaModificacion;
END;

IF COL_LENGTH(N'dbo.Vehiculos', N'Ride') IS NULL
BEGIN
    ALTER TABLE dbo.Vehiculos ADD Ride UNIQUEIDENTIFIER NULL;
    UPDATE dbo.Vehiculos SET Ride = NEWID() WHERE Ride IS NULL;
    ALTER TABLE dbo.Vehiculos ALTER COLUMN Ride UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE dbo.Vehiculos ADD CONSTRAINT DF_Vehiculos_Ride DEFAULT NEWSEQUENTIALID() FOR Ride;
END;

IF COL_LENGTH(N'dbo.Vehiculos', N'FechaModificacion') IS NULL
BEGIN
    ALTER TABLE dbo.Vehiculos ADD FechaModificacion DATETIME2(3) NULL;
    UPDATE dbo.Vehiculos SET FechaModificacion = SYSUTCDATETIME() WHERE FechaModificacion IS NULL;
    ALTER TABLE dbo.Vehiculos ALTER COLUMN FechaModificacion DATETIME2(3) NOT NULL;
    ALTER TABLE dbo.Vehiculos ADD CONSTRAINT DF_Vehiculos_FechaModificacion DEFAULT SYSUTCDATETIME() FOR FechaModificacion;
END;

IF COL_LENGTH(N'dbo.RegistrosPorteria', N'Ride') IS NULL
BEGIN
    ALTER TABLE dbo.RegistrosPorteria ADD Ride UNIQUEIDENTIFIER NULL;
    UPDATE dbo.RegistrosPorteria SET Ride = NEWID() WHERE Ride IS NULL;
    ALTER TABLE dbo.RegistrosPorteria ALTER COLUMN Ride UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE dbo.RegistrosPorteria ADD CONSTRAINT DF_RegistrosPorteria_Ride DEFAULT NEWSEQUENTIALID() FOR Ride;
END;

IF COL_LENGTH(N'dbo.RegistrosPorteria', N'FechaModificacion') IS NULL
BEGIN
    ALTER TABLE dbo.RegistrosPorteria ADD FechaModificacion DATETIME2(3) NULL;
    UPDATE dbo.RegistrosPorteria SET FechaModificacion = SYSUTCDATETIME() WHERE FechaModificacion IS NULL;
    ALTER TABLE dbo.RegistrosPorteria ALTER COLUMN FechaModificacion DATETIME2(3) NOT NULL;
    ALTER TABLE dbo.RegistrosPorteria ADD CONSTRAINT DF_RegistrosPorteria_FechaModificacion DEFAULT SYSUTCDATETIME() FOR FechaModificacion;
END;

IF COL_LENGTH(N'dbo.RegistrosParqueo', N'Ride') IS NULL
BEGIN
    ALTER TABLE dbo.RegistrosParqueo ADD Ride UNIQUEIDENTIFIER NULL;
    UPDATE dbo.RegistrosParqueo SET Ride = NEWID() WHERE Ride IS NULL;
    ALTER TABLE dbo.RegistrosParqueo ALTER COLUMN Ride UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE dbo.RegistrosParqueo ADD CONSTRAINT DF_RegistrosParqueo_Ride DEFAULT NEWSEQUENTIALID() FOR Ride;
END;

IF COL_LENGTH(N'dbo.RegistrosParqueo', N'FechaModificacion') IS NULL
BEGIN
    ALTER TABLE dbo.RegistrosParqueo ADD FechaModificacion DATETIME2(3) NULL;
    UPDATE dbo.RegistrosParqueo SET FechaModificacion = SYSUTCDATETIME() WHERE FechaModificacion IS NULL;
    ALTER TABLE dbo.RegistrosParqueo ALTER COLUMN FechaModificacion DATETIME2(3) NOT NULL;
    ALTER TABLE dbo.RegistrosParqueo ADD CONSTRAINT DF_RegistrosParqueo_FechaModificacion DEFAULT SYSUTCDATETIME() FOR FechaModificacion;
END;

IF COL_LENGTH(N'dbo.RefreshTokens', N'Ride') IS NULL
BEGIN
    ALTER TABLE dbo.RefreshTokens ADD Ride UNIQUEIDENTIFIER NULL;
    UPDATE dbo.RefreshTokens SET Ride = NEWID() WHERE Ride IS NULL;
    ALTER TABLE dbo.RefreshTokens ALTER COLUMN Ride UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE dbo.RefreshTokens ADD CONSTRAINT DF_RefreshTokens_Ride DEFAULT NEWSEQUENTIALID() FOR Ride;
END;

IF COL_LENGTH(N'dbo.RefreshTokens', N'FechaModificacion') IS NULL
BEGIN
    ALTER TABLE dbo.RefreshTokens ADD FechaModificacion DATETIME2(3) NULL;
    UPDATE dbo.RefreshTokens SET FechaModificacion = SYSUTCDATETIME() WHERE FechaModificacion IS NULL;
    ALTER TABLE dbo.RefreshTokens ALTER COLUMN FechaModificacion DATETIME2(3) NOT NULL;
    ALTER TABLE dbo.RefreshTokens ADD CONSTRAINT DF_RefreshTokens_FechaModificacion DEFAULT SYSUTCDATETIME() FOR FechaModificacion;
END;

IF COL_LENGTH(N'dbo.TiposUsuario', N'FechaRegistro') IS NULL
    ALTER TABLE dbo.TiposUsuario ADD FechaRegistro DATETIME2(3) NOT NULL CONSTRAINT DF_TiposUsuario_FechaRegistro DEFAULT SYSUTCDATETIME();

IF COL_LENGTH(N'dbo.Roles', N'FechaRegistro') IS NULL
    ALTER TABLE dbo.Roles ADD FechaRegistro DATETIME2(3) NOT NULL CONSTRAINT DF_Roles_FechaRegistro DEFAULT SYSUTCDATETIME();

IF COL_LENGTH(N'dbo.RegistrosPorteria', N'FechaRegistro') IS NULL
BEGIN
    ALTER TABLE dbo.RegistrosPorteria ADD FechaRegistro DATETIME2(3) NULL;
    UPDATE dbo.RegistrosPorteria SET FechaRegistro = FechaEntrada WHERE FechaRegistro IS NULL;
    ALTER TABLE dbo.RegistrosPorteria ALTER COLUMN FechaRegistro DATETIME2(3) NOT NULL;
    ALTER TABLE dbo.RegistrosPorteria ADD CONSTRAINT DF_RegistrosPorteria_FechaRegistro DEFAULT SYSUTCDATETIME() FOR FechaRegistro;
END;

IF COL_LENGTH(N'dbo.RegistrosParqueo', N'FechaRegistro') IS NULL
BEGIN
    ALTER TABLE dbo.RegistrosParqueo ADD FechaRegistro DATETIME2(3) NULL;
    UPDATE dbo.RegistrosParqueo SET FechaRegistro = FechaIngreso WHERE FechaRegistro IS NULL;
    ALTER TABLE dbo.RegistrosParqueo ALTER COLUMN FechaRegistro DATETIME2(3) NOT NULL;
    ALTER TABLE dbo.RegistrosParqueo ADD CONSTRAINT DF_RegistrosParqueo_FechaRegistro DEFAULT SYSUTCDATETIME() FOR FechaRegistro;
END;

IF COL_LENGTH(N'dbo.TiposUsuario', N'Estado') IS NULL
    ALTER TABLE dbo.TiposUsuario ADD Estado BIT NOT NULL CONSTRAINT DF_TiposUsuario_Estado DEFAULT 1;

IF COL_LENGTH(N'dbo.Roles', N'Estado') IS NULL
    ALTER TABLE dbo.Roles ADD Estado BIT NOT NULL CONSTRAINT DF_Roles_Estado DEFAULT 1;

IF COL_LENGTH(N'dbo.Vehiculos', N'Estado') IS NULL
    ALTER TABLE dbo.Vehiculos ADD Estado AS CONVERT(BIT, Activo) PERSISTED;

IF COL_LENGTH(N'dbo.RegistrosPorteria', N'Estado') IS NULL
    ALTER TABLE dbo.RegistrosPorteria ADD Estado AS CONVERT(BIT, CASE WHEN FechaSalida IS NULL THEN 1 ELSE 0 END) PERSISTED;

IF COL_LENGTH(N'dbo.RegistrosParqueo', N'Estado') IS NULL
    ALTER TABLE dbo.RegistrosParqueo ADD Estado AS CONVERT(BIT, CASE WHEN FechaSalida IS NULL THEN 1 ELSE 0 END) PERSISTED;

IF COL_LENGTH(N'dbo.RefreshTokens', N'Estado') IS NULL
    ALTER TABLE dbo.RefreshTokens ADD Estado AS CONVERT(BIT, CASE WHEN FechaRevocacion IS NULL THEN 1 ELSE 0 END) PERSISTED;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TiposUsuario') AND name = N'UX_TiposUsuario_Ride')
    CREATE UNIQUE INDEX UX_TiposUsuario_Ride ON dbo.TiposUsuario(Ride);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Roles') AND name = N'UX_Roles_Ride')
    CREATE UNIQUE INDEX UX_Roles_Ride ON dbo.Roles(Ride);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Usuarios') AND name = N'UX_Usuarios_Ride')
    CREATE UNIQUE INDEX UX_Usuarios_Ride ON dbo.Usuarios(Ride);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Vehiculos') AND name = N'UX_Vehiculos_Ride')
    CREATE UNIQUE INDEX UX_Vehiculos_Ride ON dbo.Vehiculos(Ride);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.RegistrosPorteria') AND name = N'UX_RegistrosPorteria_Ride')
    CREATE UNIQUE INDEX UX_RegistrosPorteria_Ride ON dbo.RegistrosPorteria(Ride);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.RegistrosParqueo') AND name = N'UX_RegistrosParqueo_Ride')
    CREATE UNIQUE INDEX UX_RegistrosParqueo_Ride ON dbo.RegistrosParqueo(Ride);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.RefreshTokens') AND name = N'UX_RefreshTokens_Ride')
    CREATE UNIQUE INDEX UX_RefreshTokens_Ride ON dbo.RefreshTokens(Ride);

EXEC('CREATE OR ALTER TRIGGER dbo.TR_TiposUsuario_Auditoria ON dbo.TiposUsuario AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;
    DECLARE @Ahora DATETIME2(3) = SYSUTCDATETIME();
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
        UPDATE t SET FechaModificacion = @Ahora FROM dbo.TiposUsuario t INNER JOIN inserted i ON i.Id = t.Id;
    INSERT dbo.AuditoriaMovimientos (Tabla, RegistroId, RegistroRide, Operacion, FechaEvento, UsuarioSql, Aplicacion, Equipo, DatosAntes, DatosDespues)
    SELECT N''TiposUsuario'', COALESCE(i.Id,d.Id), COALESCE(i.Ride,d.Ride), CASE WHEN d.Id IS NULL THEN ''INSERT'' WHEN i.Id IS NULL THEN ''DELETE'' ELSE ''UPDATE'' END, @Ahora, ORIGINAL_LOGIN(), APP_NAME(), HOST_NAME(),
        CASE WHEN d.Id IS NULL THEN NULL ELSE (SELECT d.Id, d.Nombre, d.Estado, d.Ride, d.FechaRegistro FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END,
        CASE WHEN i.Id IS NULL THEN NULL ELSE (SELECT i.Id, i.Nombre, i.Estado, i.Ride, i.FechaRegistro FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END
    FROM inserted i FULL OUTER JOIN deleted d ON i.Id = d.Id;
END;');

EXEC('CREATE OR ALTER TRIGGER dbo.TR_Roles_Auditoria ON dbo.Roles AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;
    DECLARE @Ahora DATETIME2(3) = SYSUTCDATETIME();
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
        UPDATE t SET FechaModificacion = @Ahora FROM dbo.Roles t INNER JOIN inserted i ON i.Id = t.Id;
    INSERT dbo.AuditoriaMovimientos (Tabla, RegistroId, RegistroRide, Operacion, FechaEvento, UsuarioSql, Aplicacion, Equipo, DatosAntes, DatosDespues)
    SELECT N''Roles'', COALESCE(i.Id,d.Id), COALESCE(i.Ride,d.Ride), CASE WHEN d.Id IS NULL THEN ''INSERT'' WHEN i.Id IS NULL THEN ''DELETE'' ELSE ''UPDATE'' END, @Ahora, ORIGINAL_LOGIN(), APP_NAME(), HOST_NAME(),
        CASE WHEN d.Id IS NULL THEN NULL ELSE (SELECT d.Id, d.Nombre, d.Estado, d.Ride, d.FechaRegistro FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END,
        CASE WHEN i.Id IS NULL THEN NULL ELSE (SELECT i.Id, i.Nombre, i.Estado, i.Ride, i.FechaRegistro FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END
    FROM inserted i FULL OUTER JOIN deleted d ON i.Id = d.Id;
END;');

EXEC('CREATE OR ALTER TRIGGER dbo.TR_Usuarios_Auditoria ON dbo.Usuarios AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;
    DECLARE @Ahora DATETIME2(3) = SYSUTCDATETIME();
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
        UPDATE t SET FechaModificacion = @Ahora FROM dbo.Usuarios t INNER JOIN inserted i ON i.Id = t.Id;
    INSERT dbo.AuditoriaMovimientos (Tabla, RegistroId, RegistroRide, Operacion, FechaEvento, UsuarioSql, Aplicacion, Equipo, DatosAntes, DatosDespues)
    SELECT N''Usuarios'', COALESCE(i.Id,d.Id), COALESCE(i.Ride,d.Ride), CASE WHEN d.Id IS NULL THEN ''INSERT'' WHEN i.Id IS NULL THEN ''DELETE'' ELSE ''UPDATE'' END, @Ahora, ORIGINAL_LOGIN(), APP_NAME(), HOST_NAME(),
        CASE WHEN d.Id IS NULL THEN NULL ELSE (SELECT d.Id, d.NombreCompleto, d.DocumentoIdentidad, d.TipoUsuarioId, d.RolId, d.FotoUrl, d.Estado, d.IntentosFallidos, d.BloqueoHasta, d.Ride, d.FechaRegistro FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END,
        CASE WHEN i.Id IS NULL THEN NULL ELSE (SELECT i.Id, i.NombreCompleto, i.DocumentoIdentidad, i.TipoUsuarioId, i.RolId, i.FotoUrl, i.Estado, i.IntentosFallidos, i.BloqueoHasta, i.Ride, i.FechaRegistro FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END
    FROM inserted i FULL OUTER JOIN deleted d ON i.Id = d.Id;
END;');

EXEC('CREATE OR ALTER TRIGGER dbo.TR_Vehiculos_Auditoria ON dbo.Vehiculos AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;
    DECLARE @Ahora DATETIME2(3) = SYSUTCDATETIME();
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
        UPDATE t SET FechaModificacion = @Ahora FROM dbo.Vehiculos t INNER JOIN inserted i ON i.Id = t.Id;
    INSERT dbo.AuditoriaMovimientos (Tabla, RegistroId, RegistroRide, Operacion, FechaEvento, UsuarioSql, Aplicacion, Equipo, DatosAntes, DatosDespues)
    SELECT N''Vehiculos'', COALESCE(i.Id,d.Id), COALESCE(i.Ride,d.Ride), CASE WHEN d.Id IS NULL THEN ''INSERT'' WHEN i.Id IS NULL THEN ''DELETE'' ELSE ''UPDATE'' END, @Ahora, ORIGINAL_LOGIN(), APP_NAME(), HOST_NAME(),
        CASE WHEN d.Id IS NULL THEN NULL ELSE (SELECT d.Id, d.UsuarioId, d.Matricula, d.Marca, d.Modelo, d.Color, d.Activo, d.Estado, d.Ride, d.FechaRegistro FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END,
        CASE WHEN i.Id IS NULL THEN NULL ELSE (SELECT i.Id, i.UsuarioId, i.Matricula, i.Marca, i.Modelo, i.Color, i.Activo, i.Estado, i.Ride, i.FechaRegistro FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END
    FROM inserted i FULL OUTER JOIN deleted d ON i.Id = d.Id;
END;');

EXEC('CREATE OR ALTER TRIGGER dbo.TR_RegistrosPorteria_Auditoria ON dbo.RegistrosPorteria AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;
    DECLARE @Ahora DATETIME2(3) = SYSUTCDATETIME();
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
        UPDATE t SET FechaModificacion = @Ahora FROM dbo.RegistrosPorteria t INNER JOIN inserted i ON i.Id = t.Id;
    INSERT dbo.AuditoriaMovimientos (Tabla, RegistroId, RegistroRide, Operacion, FechaEvento, UsuarioSql, Aplicacion, Equipo, DatosAntes, DatosDespues)
    SELECT N''RegistrosPorteria'', COALESCE(i.Id,d.Id), COALESCE(i.Ride,d.Ride), CASE WHEN d.Id IS NULL THEN ''INSERT'' WHEN i.Id IS NULL THEN ''DELETE'' ELSE ''UPDATE'' END, @Ahora, ORIGINAL_LOGIN(), APP_NAME(), HOST_NAME(),
        CASE WHEN d.Id IS NULL THEN NULL ELSE (SELECT d.Id, d.UsuarioId, d.FechaEntrada, d.FechaSalida, d.PuertaEntrada, d.PuertaSalida, d.MotivoVisita, d.AreaDestino, d.Estado, d.Ride, d.FechaRegistro FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END,
        CASE WHEN i.Id IS NULL THEN NULL ELSE (SELECT i.Id, i.UsuarioId, i.FechaEntrada, i.FechaSalida, i.PuertaEntrada, i.PuertaSalida, i.MotivoVisita, i.AreaDestino, i.Estado, i.Ride, i.FechaRegistro FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END
    FROM inserted i FULL OUTER JOIN deleted d ON i.Id = d.Id;
END;');

EXEC('CREATE OR ALTER TRIGGER dbo.TR_RegistrosParqueo_Auditoria ON dbo.RegistrosParqueo AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;
    DECLARE @Ahora DATETIME2(3) = SYSUTCDATETIME();
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
        UPDATE t SET FechaModificacion = @Ahora FROM dbo.RegistrosParqueo t INNER JOIN inserted i ON i.Id = t.Id;
    INSERT dbo.AuditoriaMovimientos (Tabla, RegistroId, RegistroRide, Operacion, FechaEvento, UsuarioSql, Aplicacion, Equipo, DatosAntes, DatosDespues)
    SELECT N''RegistrosParqueo'', COALESCE(i.Id,d.Id), COALESCE(i.Ride,d.Ride), CASE WHEN d.Id IS NULL THEN ''INSERT'' WHEN i.Id IS NULL THEN ''DELETE'' ELSE ''UPDATE'' END, @Ahora, ORIGINAL_LOGIN(), APP_NAME(), HOST_NAME(),
        CASE WHEN d.Id IS NULL THEN NULL ELSE (SELECT d.Id, d.VehiculoId, d.FechaIngreso, d.FechaSalida, d.PuertaAcceso, d.Observaciones, d.Estado, d.Ride, d.FechaRegistro FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END,
        CASE WHEN i.Id IS NULL THEN NULL ELSE (SELECT i.Id, i.VehiculoId, i.FechaIngreso, i.FechaSalida, i.PuertaAcceso, i.Observaciones, i.Estado, i.Ride, i.FechaRegistro FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END
    FROM inserted i FULL OUTER JOIN deleted d ON i.Id = d.Id;
END;');

EXEC('CREATE OR ALTER TRIGGER dbo.TR_RefreshTokens_Auditoria ON dbo.RefreshTokens AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;
    DECLARE @Ahora DATETIME2(3) = SYSUTCDATETIME();
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
        UPDATE t SET FechaModificacion = @Ahora FROM dbo.RefreshTokens t INNER JOIN inserted i ON i.Id = t.Id;
    INSERT dbo.AuditoriaMovimientos (Tabla, RegistroId, RegistroRide, Operacion, FechaEvento, UsuarioSql, Aplicacion, Equipo, DatosAntes, DatosDespues)
    SELECT N''RefreshTokens'', COALESCE(i.Id,d.Id), COALESCE(i.Ride,d.Ride), CASE WHEN d.Id IS NULL THEN ''INSERT'' WHEN i.Id IS NULL THEN ''DELETE'' ELSE ''UPDATE'' END, @Ahora, ORIGINAL_LOGIN(), APP_NAME(), HOST_NAME(),
        CASE WHEN d.Id IS NULL THEN NULL ELSE (SELECT d.Id, d.UsuarioId, d.FechaCreacion, d.FechaExpiracion, d.FechaRevocacion, d.Estado, d.Ride FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END,
        CASE WHEN i.Id IS NULL THEN NULL ELSE (SELECT i.Id, i.UsuarioId, i.FechaCreacion, i.FechaExpiracion, i.FechaRevocacion, i.Estado, i.Ride FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END
    FROM inserted i FULL OUTER JOIN deleted d ON i.Id = d.Id;
END;');

PRINT 'Auditoría estándar instalada correctamente.';
