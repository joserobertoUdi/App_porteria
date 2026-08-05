BEGIN TRANSACTION;
GO

ALTER TABLE [RefreshTokens] DROP CONSTRAINT [FK_RefreshTokens_Usuarios_UsuarioId];
GO

ALTER TABLE [RegistrosParqueo] DROP CONSTRAINT [FK_RegistrosParqueo_Vehiculos_VehiculoId];
GO

ALTER TABLE [RegistrosPorteria] DROP CONSTRAINT [FK_RegistrosPorteria_Usuarios_UsuarioId];
GO

ALTER TABLE [Usuarios] DROP CONSTRAINT [FK_Usuarios_TiposUsuario_TipoUsuarioId];
GO

ALTER TABLE [Vehiculos] DROP CONSTRAINT [FK_Vehiculos_Usuarios_UsuarioId];
GO

CREATE UNIQUE INDEX [IX_TiposUsuario_Nombre] ON [TiposUsuario] ([Nombre]);
GO

CREATE UNIQUE INDEX [IX_Roles_Nombre] ON [Roles] ([Nombre]);
GO

ALTER TABLE [RegistrosPorteria] ADD CONSTRAINT [CK_RegistrosPorteria_Fechas] CHECK ([FechaSalida] IS NULL OR [FechaSalida] >= [FechaEntrada]);
GO

ALTER TABLE [RegistrosParqueo] ADD CONSTRAINT [CK_RegistrosParqueo_Fechas] CHECK ([FechaSalida] IS NULL OR [FechaSalida] >= [FechaIngreso]);
GO

ALTER TABLE [RefreshTokens] ADD CONSTRAINT [CK_RefreshTokens_Fechas] CHECK ([FechaExpiracion] > [FechaCreacion] AND ([FechaRevocacion] IS NULL OR [FechaRevocacion] >= [FechaCreacion]));
GO

ALTER TABLE [RefreshTokens] ADD CONSTRAINT [FK_RefreshTokens_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [RegistrosParqueo] ADD CONSTRAINT [FK_RegistrosParqueo_Vehiculos_VehiculoId] FOREIGN KEY ([VehiculoId]) REFERENCES [Vehiculos] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [RegistrosPorteria] ADD CONSTRAINT [FK_RegistrosPorteria_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Usuarios] ADD CONSTRAINT [FK_Usuarios_TiposUsuario_TipoUsuarioId] FOREIGN KEY ([TipoUsuarioId]) REFERENCES [TiposUsuario] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Vehiculos] ADD CONSTRAINT [FK_Vehiculos_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260805140230_EnforceIntegrityAndRestrictDeletes', N'8.0.11');
GO

COMMIT;
GO

