using Dapper;
using Microsoft.Data.SqlClient;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Infraestructura.Data;
using ServiciosGenerales.Infraestructura.Repositories;
using Xunit;

namespace ServiciosGenerales.Tests;

/// <summary>
/// Tests de integración para los repositorios Dapper.
/// Cada test crea una base de datos temporal en LocalDB, aplica el
/// esquema con SQL directo, ejecuta los repos y limpia al final.
///
/// Estos tests VALIDAN que las consultas Dapper producen los mismos
/// resultados que la implementación previa de EF Core.
/// </summary>
[Collection("IntegrationTests")]
public class DapperRepositoriesIntegrationTests : IDisposable
{
    private static readonly string BaseConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly string _dbName;
    private readonly string _connectionString;
    private readonly DapperContext _dapper;

    public DapperRepositoriesIntegrationTests()
    {
        _dbName = $"DapperTests_{Guid.NewGuid():N}";
        _connectionString = $"{BaseConnectionString}Database={_dbName};";
        _dapper = new DapperContext(_connectionString);

        // Crear la base de datos en master.
        using (var masterConn = new SqlConnection(BaseConnectionString))
        {
            masterConn.Open();
            masterConn.Execute($"CREATE DATABASE [{_dbName}]");
        }

        // Aplicar esquema y sembrar datos en la nueva base de datos.
        using (var dbConn = new SqlConnection(_connectionString))
        {
            AplicarEsquema(dbConn);
            SembrarDatos(dbConn);
        }
    }

    public void Dispose()
    {
        using var conn = new SqlConnection(BaseConnectionString);
        conn.Open();

        // Forzar cierre de conexiones activas antes de DROP.
        conn.Execute($@"
            ALTER DATABASE [{_dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE [{_dbName}];");
    }

    // ────────────────────────────────────────────────────────
    //  Esquema SQL replicado del DbContext de EF Core
    // ────────────────────────────────────────────────────────

    private void AplicarEsquema(SqlConnection conn)
    {
        conn.Execute("""
            CREATE TABLE TiposUsuario (
                Id      INT IDENTITY(1,1) PRIMARY KEY,
                Nombre  NVARCHAR(50) NOT NULL UNIQUE
            );

            CREATE TABLE Roles (
                Id      INT IDENTITY(1,1) PRIMARY KEY,
                Nombre  NVARCHAR(50) NOT NULL UNIQUE
            );

            CREATE TABLE Usuarios (
                Id                 INT IDENTITY(1,1) PRIMARY KEY,
                NombreCompleto     NVARCHAR(200) NOT NULL,
                DocumentoIdentidad NVARCHAR(20)  NOT NULL UNIQUE,
                TipoUsuarioId      INT NOT NULL,
                FotoUrl            NVARCHAR(500) NULL,
                PasswordHash       NVARCHAR(500) NULL,
                RolId              INT NULL,
                FechaRegistro      DATETIME2 NOT NULL DEFAULT GETDATE(),
                Estado             BIT NOT NULL DEFAULT 1,
                IntentosFallidos   INT NOT NULL DEFAULT 0,
                BloqueoHasta       DATETIME2 NULL,
                CONSTRAINT FK_Usuarios_TiposUsuario FOREIGN KEY (TipoUsuarioId) REFERENCES TiposUsuario(Id),
                CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RolId) REFERENCES Roles(Id)
            );

            CREATE TABLE RegistrosPorteria (
                Id                     INT IDENTITY(1,1) PRIMARY KEY,
                UsuarioId              INT NOT NULL,
                RegistradoPorUsuarioId INT NULL,
                FechaEntrada           DATETIME2 NOT NULL DEFAULT GETDATE(),
                FechaSalida            DATETIME2 NULL,
                PuertaEntrada          NVARCHAR(50) NULL,
                PuertaSalida           NVARCHAR(50) NULL,
                MotivoVisita           NVARCHAR(500) NULL,
                AreaDestino            NVARCHAR(200) NULL,
                FotoUrl                NVARCHAR(200) NULL,
                CONSTRAINT FK_RegistrosPorteria_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
                CONSTRAINT FK_RegistrosPorteria_RegistradoPor FOREIGN KEY (RegistradoPorUsuarioId) REFERENCES Usuarios(Id),
                CONSTRAINT CK_RegistrosPorteria_Fechas CHECK (FechaSalida IS NULL OR FechaSalida >= FechaEntrada)
            );

            CREATE TABLE Vehiculos (
                Id            INT IDENTITY(1,1) PRIMARY KEY,
                UsuarioId     INT NOT NULL,
                Matricula     NVARCHAR(20) NOT NULL UNIQUE,
                Marca         NVARCHAR(50) NULL,
                Modelo        NVARCHAR(50) NULL,
                Color         NVARCHAR(30) NULL,
                Activo        BIT NOT NULL DEFAULT 1,
                FechaRegistro DATETIME2 NOT NULL DEFAULT GETDATE(),
                CONSTRAINT FK_Vehiculos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
            );

            CREATE TABLE RegistrosParqueo (
                Id                     INT IDENTITY(1,1) PRIMARY KEY,
                VehiculoId             INT NOT NULL,
                RegistradoPorUsuarioId INT NULL,
                FechaIngreso           DATETIME2 NOT NULL DEFAULT GETDATE(),
                FechaSalida            DATETIME2 NULL,
                PuertaAcceso           NVARCHAR(50) NOT NULL,
                Observaciones          NVARCHAR(255) NULL,
                CONSTRAINT FK_RegistrosParqueo_Vehiculos FOREIGN KEY (VehiculoId) REFERENCES Vehiculos(Id),
                CONSTRAINT FK_RegistrosParqueo_RegistradoPor FOREIGN KEY (RegistradoPorUsuarioId) REFERENCES Usuarios(Id),
                CONSTRAINT CK_RegistrosParqueo_Fechas CHECK (FechaSalida IS NULL OR FechaSalida >= FechaIngreso)
            );

            CREATE TABLE RefreshTokens (
                Id              INT IDENTITY(1,1) PRIMARY KEY,
                UsuarioId       INT NOT NULL,
                TokenHash       NVARCHAR(64) NOT NULL UNIQUE,
                FechaExpiracion DATETIME2 NOT NULL,
                FechaCreacion   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                FechaRevocacion DATETIME2 NULL,
                ReemplazadoPor  NVARCHAR(64) NULL,
                CONSTRAINT FK_RefreshTokens_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
                CONSTRAINT CK_RefreshTokens_Fechas CHECK (
                    FechaExpiracion > FechaCreacion
                    AND (FechaRevocacion IS NULL OR FechaRevocacion >= FechaCreacion)
                )
            );
            """);
    }

    // ────────────────────────────────────────────────────────
    //  Datos de prueba
    // ────────────────────────────────────────────────────────

    private void SembrarDatos(SqlConnection conn)
    {
        conn.Execute("INSERT INTO TiposUsuario (Nombre) VALUES ('Estudiante'), ('Trabajador'), ('Visitante')");
        conn.Execute("INSERT INTO Roles (Nombre) VALUES ('Administrador'), ('PorteroPorteria')");

        conn.Execute("""
            INSERT INTO Usuarios
                (NombreCompleto, DocumentoIdentidad, TipoUsuarioId, RolId, Estado, PasswordHash)
            VALUES
                ('Juan Perez',    '1001', 1, 1, 1, 'hash1'),
                ('Maria Garcia',  '1002', 2, 2, 1, 'hash2'),
                ('Pedro Lopez',   '1003', 3, NULL, 1, NULL),
                ('Ana Inactiva',  '1004', 1, NULL, 0, 'hash4')
            """);

        conn.Execute("""
            INSERT INTO Vehiculos (UsuarioId, Matricula, Marca, Modelo, Color)
            VALUES
                (2, 'ABC-1234', 'Toyota', 'Corolla', 'Blanco'),
                (2, 'XYZ-5678', 'Honda',  'Civic',   'Negro')
            """);

        conn.Execute("""
            INSERT INTO RegistrosPorteria
                (UsuarioId, RegistradoPorUsuarioId, FechaEntrada, PuertaEntrada, MotivoVisita, AreaDestino)
            VALUES
                (1, 2, '2026-08-01 08:00:00', 'Principal', 'Clase',      'Aulas'),
                (2, 1, '2026-08-01 09:00:00', 'Principal', 'Trabajo',    'Oficina'),
                (2, 1, '2026-08-02 08:00:00', 'Parqueo',   'Reunion',    'Admin')
            """);

        // Salida solo del primer registro.
        conn.Execute("""
            UPDATE RegistrosPorteria
            SET FechaSalida = '2026-08-01 12:00:00', PuertaSalida = 'Principal'
            WHERE Id = 1
            """);

        conn.Execute("""
            INSERT INTO RegistrosParqueo
                (VehiculoId, RegistradoPorUsuarioId, FechaIngreso, PuertaAcceso, Observaciones)
            VALUES
                (1, 2, '2026-08-01 08:30:00', 'Principal', 'Estacionamiento A'),
                (2, 1, '2026-08-02 09:00:00', 'Principal', NULL)
            """);

        conn.Execute("""
            INSERT INTO RegistrosParqueo
                (VehiculoId, RegistradoPorUsuarioId, FechaIngreso, FechaSalida, PuertaAcceso)
            VALUES
                (1, 1, '2026-08-01 08:30:00', '2026-08-01 17:00:00', 'Principal')
            """);
    }

    // ────────────────────────────────────────────────────────
    //  TiposUsuarioRepository
    // ────────────────────────────────────────────────────────

    [Fact]
    public async Task TiposUsuario_ObtenerTodos_Devuelve3Registros()
    {
        var repo = new TiposUsuarioRepository(_dapper);

        var resultado = (await repo.ObtenerTodosAsync()).ToList();

        Assert.Equal(3, resultado.Count);
        Assert.Equal("Estudiante", resultado[0].Nombre);
        Assert.Equal("Trabajador", resultado[1].Nombre);
        Assert.Equal("Visitante", resultado[2].Nombre);
    }

    // ────────────────────────────────────────────────────────
    //  UsuarioRepository
    // ────────────────────────────────────────────────────────

    [Fact]
    public async Task Usuario_ObtenerPorDocumento_ConRol()
    {
        var repo = new UsuarioRepository(_dapper);

        var usuario = await repo.ObtenerPorDocumentoAsync("1001");

        Assert.NotNull(usuario);
        Assert.Equal("Juan Perez", usuario!.NombreCompleto);
        Assert.NotNull(usuario.Rol);
        Assert.Equal("Administrador", usuario.Rol!.Nombre);
    }

    [Fact]
    public async Task Usuario_ObtenerPorDocumento_SinRol()
    {
        var repo = new UsuarioRepository(_dapper);

        var usuario = await repo.ObtenerPorDocumentoAsync("1003");

        Assert.NotNull(usuario);
        Assert.Null(usuario!.Rol);
    }

    [Fact]
    public async Task Usuario_ObtenerPorDocumento_NoExiste_DevuelveNull()
    {
        var repo = new UsuarioRepository(_dapper);

        var usuario = await repo.ObtenerPorDocumentoAsync("9999");

        Assert.Null(usuario);
    }

    [Fact]
    public async Task Usuario_ObtenerPorId_ConRol()
    {
        var repo = new UsuarioRepository(_dapper);

        var usuario = await repo.ObtenerPorIdAsync(2);

        Assert.NotNull(usuario);
        Assert.Equal("Maria Garcia", usuario!.NombreCompleto);
        Assert.NotNull(usuario.Rol);
        Assert.Equal("PorteroPorteria", usuario.Rol!.Nombre);
    }

    [Fact]
    public async Task Usuario_ObtenerTodos_ConRolYTipo()
    {
        var repo = new UsuarioRepository(_dapper);

        var usuarios = (await repo.ObtenerTodosAsync()).ToList();

        Assert.Equal(4, usuarios.Count);
        Assert.All(usuarios, u => Assert.NotNull(u.TipoUsuario));
    }

    [Fact]
    public async Task Usuario_ObtenerCoincidencia_PorDocumento()
    {
        var repo = new UsuarioRepository(_dapper);

        var coincidencias = (await repo.ObtenerCoincidenciaAsync("100")).ToList();

        // Solo 3: Ana Inactiva tiene Estado=0 y se excluye.
        Assert.Equal(3, coincidencias.Count);
    }

    [Fact]
    public async Task Usuario_ObtenerCoincidencia_PorNombre()
    {
        var repo = new UsuarioRepository(_dapper);

        var coincidencias = (await repo.ObtenerCoincidenciaAsync("Maria")).ToList();

        Assert.Single(coincidencias);
        Assert.Equal("Maria Garcia", coincidencias[0].NombreCompleto);
    }

    [Fact]
    public async Task Usuario_Crear_Y_ObtenerPorDocumento()
    {
        var repo = new UsuarioRepository(_dapper);
        var nuevo = new Usuario
        {
            NombreCompleto = "Test Dapper",
            DocumentoIdentidad = "9999",
            TipoUsuarioId = 1,
            Estado = true,
            FechaRegistro = DateTime.Now
        };

        var id = await repo.CrearAsync(nuevo);

        Assert.True(id > 0);
        var encontrado = await repo.ObtenerPorDocumentoAsync("9999");
        Assert.NotNull(encontrado);
        Assert.Equal("Test Dapper", encontrado!.NombreCompleto);
    }

    [Fact]
    public async Task Usuario_Actualizar_Estado()
    {
        var repo = new UsuarioRepository(_dapper);
        var usuario = await repo.ObtenerPorIdAsync(4);
        Assert.NotNull(usuario);

        usuario!.Estado = false;
        var ok = await repo.ActualizarAsync(usuario);

        Assert.True(ok);
        var actualizado = await repo.ObtenerPorIdAsync(4);
        Assert.NotNull(actualizado);
        Assert.False(actualizado!.Estado);
    }

    // ────────────────────────────────────────────────────────
    //  RegistroPorteriaRepository
    // ────────────────────────────────────────────────────────

    [Fact]
    public async Task RegistroPorteria_RegistrarEntrada()
    {
        var repo = new RegistroPorteriaRepository(_dapper);
        var registro = new RegistroPorteria
        {
            UsuarioId = 3,
            FechaEntrada = DateTime.Now,
            PuertaEntrada = "Parqueo",
            MotivoVisita = "Visita"
        };

        var id = await repo.RegistrarEntradaAsync(registro);

        Assert.True(id > 0);
        Assert.Equal(id, registro.Id);
    }

    [Fact]
    public async Task RegistroPorteria_RegistrarSalida()
    {
        var repo = new RegistroPorteriaRepository(_dapper);

        var ok = await repo.RegistrarSalidaAsync(2, DateTime.Now, "Principal");

        Assert.True(ok);
    }

    [Fact]
    public async Task RegistroPorteria_RegistrarSalida_NoExiste_DevuelveFalse()
    {
        var repo = new RegistroPorteriaRepository(_dapper);

        var ok = await repo.RegistrarSalidaAsync(999, DateTime.Now, null);

        Assert.False(ok);
    }

    [Fact]
    public async Task RegistroPorteria_ObtenerVisitasActivas_SoloSinSalida()
    {
        var repo = new RegistroPorteriaRepository(_dapper);

        var activas = (await repo.ObtenerVisitasActivasAsync()).ToList();

        Assert.Equal(2, activas.Count);
        Assert.All(activas, r => Assert.Null(r.FechaSalida));
        Assert.All(activas, r => Assert.NotNull(r.Usuario));
    }

    [Fact]
    public async Task RegistroPorteria_ObtenerHistorial_TodosLosRegistros()
    {
        var repo = new RegistroPorteriaRepository(_dapper);

        var historial = (await repo.ObtenerHistorialAsync()).ToList();

        Assert.Equal(3, historial.Count);
        Assert.All(historial, r => Assert.NotNull(r.Usuario));
        Assert.All(historial, r => Assert.NotNull(r.RegistradoPor));
    }

    [Fact]
    public async Task RegistroPorteria_ObtenerPorRegistradoPorId()
    {
        var repo = new RegistroPorteriaRepository(_dapper);

        var registros = (await repo.ObtenerPorRegistradoPorIdAsync(2)).ToList();

        // Solo el registro 1 fue aprobado por Maria (Id=2).
        Assert.Single(registros);
        var r = registros.Single();
        Assert.NotNull(r.RegistradoPor);
        Assert.Equal("Maria Garcia", r.RegistradoPor!.NombreCompleto);
        Assert.NotNull(r.Usuario);
        Assert.Equal("Juan Perez", r.Usuario!.NombreCompleto);
    }

    [Fact]
    public async Task RegistroPorteria_ObtenerPorRegistradoPorId_SinAprobaciones()
    {
        var repo = new RegistroPorteriaRepository(_dapper);

        var registros = (await repo.ObtenerPorRegistradoPorIdAsync(4)).ToList();

        // Ana (Id=4) está inactiva y nunca aprobó nada.
        Assert.Empty(registros);
    }

    [Fact]
    public async Task RegistroPorteria_RegistrarEntrada_ConOperador()
    {
        var repo = new RegistroPorteriaRepository(_dapper);
        var registro = new RegistroPorteria
        {
            UsuarioId = 3,
            RegistradoPorUsuarioId = 2,
            FechaEntrada = DateTime.Now,
            PuertaEntrada = "Parqueo",
            MotivoVisita = "Visita"
        };

        var id = await repo.RegistrarEntradaAsync(registro);

        Assert.True(id > 0);
        var aprobadasPorMaria = (await repo.ObtenerPorRegistradoPorIdAsync(2)).ToList();
        Assert.Equal(2, aprobadasPorMaria.Count); // registro 1 + el nuevo
        Assert.All(aprobadasPorMaria, r => Assert.NotNull(r.RegistradoPor));
    }

    [Fact]
    public async Task RegistroPorteria_ObtenerPorUsuarioId()
    {
        var repo = new RegistroPorteriaRepository(_dapper);

        var registros = (await repo.ObtenerPorUsuarioIdAsync(2)).ToList();

        Assert.Equal(2, registros.Count);
        Assert.All(registros, r => Assert.Equal(2, r.UsuarioId));
    }

    // ────────────────────────────────────────────────────────
    //  VehiculoRepository
    // ────────────────────────────────────────────────────────

    [Fact]
    public async Task Vehiculo_ObtenerPorId()
    {
        var repo = new VehiculoRepository(_dapper);

        var vehiculo = await repo.ObtenerPorIdAsync(1);

        Assert.NotNull(vehiculo);
        Assert.Equal("ABC-1234", vehiculo!.Matricula);
        Assert.Equal("Toyota", vehiculo.Marca);
    }

    [Fact]
    public async Task Vehiculo_ObtenerPorMatricula_ConUsuario()
    {
        var repo = new VehiculoRepository(_dapper);

        var vehiculo = await repo.ObtenerPorMatriculaAsync("ABC-1234");

        Assert.NotNull(vehiculo);
        Assert.NotNull(vehiculo!.Usuario);
        Assert.Equal("Maria Garcia", vehiculo.Usuario!.NombreCompleto);
    }

    [Fact]
    public async Task Vehiculo_ObtenerPorMatricula_NoExiste_DevuelveNull()
    {
        var repo = new VehiculoRepository(_dapper);

        var vehiculo = await repo.ObtenerPorMatriculaAsync("NO-EXISTE");

        Assert.Null(vehiculo);
    }

    [Fact]
    public async Task Vehiculo_ObtenerPorUsuarioId()
    {
        var repo = new VehiculoRepository(_dapper);

        var vehiculos = (await repo.ObtenerPorUsuarioIdAsync(2)).ToList();

        Assert.Equal(2, vehiculos.Count);
        Assert.All(vehiculos, v => Assert.NotNull(v.Usuario));
    }

    [Fact]
    public async Task Vehiculo_Crear_Y_Obtener()
    {
        var repo = new VehiculoRepository(_dapper);
        var nuevo = new Vehiculo
        {
            UsuarioId = 3,
            Matricula = "NEW-0001",
            Marca = "Ford",
            Modelo = "Escape",
            Color = "Rojo",
            Activo = true,
            FechaRegistro = DateTime.Now
        };

        var id = await repo.CrearAsync(nuevo);

        Assert.True(id > 0);
        var encontrado = await repo.ObtenerPorMatriculaAsync("NEW-0001");
        Assert.NotNull(encontrado);
        Assert.Equal("Ford", encontrado!.Marca);
    }

    [Fact]
    public async Task Vehiculo_Actualizar_Color()
    {
        var repo = new VehiculoRepository(_dapper);
        var vehiculo = await repo.ObtenerPorIdAsync(1);
        Assert.NotNull(vehiculo);

        vehiculo!.Color = "Azul";
        var ok = await repo.ActualizarAsync(vehiculo);

        Assert.True(ok);
        var actualizado = await repo.ObtenerPorIdAsync(1);
        Assert.Equal("Azul", actualizado!.Color);
    }

    // ────────────────────────────────────────────────────────
    //  RegistroParqueoRepository
    // ────────────────────────────────────────────────────────

    [Fact]
    public async Task RegistroParqueo_RegistrarEntrada()
    {
        var repo = new RegistroParqueoRepository(_dapper);
        var registro = new RegistroParqueo
        {
            VehiculoId = 1,
            FechaIngreso = DateTime.Now,
            PuertaAcceso = "Principal"
        };

        var id = await repo.RegistrarEntradaAsync(registro);

        Assert.True(id > 0);
    }

    [Fact]
    public async Task RegistroParqueo_RegistrarSalida()
    {
        var repo = new RegistroParqueoRepository(_dapper);

        var ok = await repo.RegistrarSalidaAsync(2, DateTime.Now);

        Assert.True(ok);
    }

    [Fact]
    public async Task RegistroParqueo_RegistrarSalida_NoExiste_DevuelveFalse()
    {
        var repo = new RegistroParqueoRepository(_dapper);

        var ok = await repo.RegistrarSalidaAsync(999, DateTime.Now);

        Assert.False(ok);
    }

    [Fact]
    public async Task RegistroParqueo_ObtenerActivos()
    {
        var repo = new RegistroParqueoRepository(_dapper);

        var activos = (await repo.ObtenerActivosAsync()).ToList();

        // 2 activos (Id=1 e Id=2 sin salida); Id=3 tiene salida.
        Assert.Equal(2, activos.Count);
        Assert.All(activos, r => Assert.Null(r.FechaSalida));
        Assert.All(activos, r => Assert.NotNull(r.Vehiculo));
        Assert.All(activos, r => Assert.NotNull(r.Vehiculo!.Usuario));
    }

    [Fact]
    public async Task RegistroParqueo_ObtenerPorVehiculoId()
    {
        var repo = new RegistroParqueoRepository(_dapper);

        var registros = (await repo.ObtenerPorVehiculoIdAsync(1)).ToList();

        Assert.Equal(2, registros.Count);
        Assert.All(registros, r => Assert.NotNull(r.Vehiculo));
    }

    [Fact]
    public async Task RegistroParqueo_ObtenerPorUsuarioId()
    {
        var repo = new RegistroParqueoRepository(_dapper);

        var registros = (await repo.ObtenerPorUsuarioIdAsync(2)).ToList();

        Assert.Equal(3, registros.Count);
        Assert.All(registros, r =>
        {
            Assert.NotNull(r.Vehiculo);
            Assert.NotNull(r.Vehiculo!.Usuario);
        });
    }

    [Fact]
    public async Task RegistroParqueo_ObtenerHistorial()
    {
        var repo = new RegistroParqueoRepository(_dapper);

        var historial = (await repo.ObtenerHistorialAsync()).ToList();

        Assert.Equal(3, historial.Count);
        Assert.All(historial, r =>
        {
            Assert.NotNull(r.Vehiculo);
            Assert.NotNull(r.Vehiculo!.Usuario);
            Assert.NotNull(r.RegistradoPor);
        });
    }

    [Fact]
    public async Task RegistroParqueo_ObtenerPorRegistradoPorId()
    {
        var repo = new RegistroParqueoRepository(_dapper);

        var registros = (await repo.ObtenerPorRegistradoPorIdAsync(2)).ToList();

        // Solo el registros 1 (vehículo Id=1) fue aprobado por Maria (Id=2).
        Assert.Single(registros);
        var r = registros.Single();
        Assert.NotNull(r.RegistradoPor);
        Assert.Equal("Maria Garcia", r.RegistradoPor!.NombreCompleto);
        Assert.NotNull(r.Vehiculo);
        Assert.Equal("ABC-1234", r.Vehiculo!.Matricula);
        Assert.NotNull(r.Vehiculo.Usuario);
    }

    [Fact]
    public async Task RegistroParqueo_RegistrarEntrada_ConOperador()
    {
        var repo = new RegistroParqueoRepository(_dapper);
        var registro = new RegistroParqueo
        {
            VehiculoId = 1,
            RegistradoPorUsuarioId = 3,
            FechaIngreso = DateTime.Now,
            PuertaAcceso = "Lateral"
        };

        var id = await repo.RegistrarEntradaAsync(registro);

        Assert.True(id > 0);
        var aprobadasPorPedro = (await repo.ObtenerPorRegistradoPorIdAsync(3)).ToList();
        Assert.Single(aprobadasPorPedro);
        Assert.Equal("Pedro Lopez", aprobadasPorPedro.Single().RegistradoPor!.NombreCompleto);
    }

    // ────────────────────────────────────────────────────────
    //  RefreshTokenRepository
    // ────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_Agregar_Y_ObtenerPorHash_ConUsuario()
    {
        var repo = new RefreshTokenRepository(_dapper);
        var token = new RefreshToken
        {
            UsuarioId = 1,
            TokenHash = "abc123hash",
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
            FechaCreacion = DateTime.UtcNow
        };

        await repo.AgregarAsync(token);

        Assert.True(token.Id > 0);

        var encontrado = await repo.ObtenerPorHashAsync("abc123hash");
        Assert.NotNull(encontrado);
        Assert.Equal(1, encontrado!.UsuarioId);
        Assert.NotNull(encontrado.Usuario);
        Assert.Equal("Juan Perez", encontrado.Usuario!.NombreCompleto);
        Assert.NotNull(encontrado.Usuario.Rol);
    }

    [Fact]
    public async Task RefreshToken_ObtenerPorHash_NoExiste_DevuelveNull()
    {
        var repo = new RefreshTokenRepository(_dapper);

        var token = await repo.ObtenerPorHashAsync("no-existe");

        Assert.Null(token);
    }

    [Fact]
    public async Task RefreshToken_Revocar()
    {
        var repo = new RefreshTokenRepository(_dapper);
        var token = new RefreshToken
        {
            UsuarioId = 2,
            TokenHash = "hash-revocar",
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
            FechaCreacion = DateTime.UtcNow
        };
        await repo.AgregarAsync(token);

        await repo.RevocarAsync(token, "nuevo-hash");

        var encontrado = await repo.ObtenerPorHashAsync("hash-revocar");
        Assert.NotNull(encontrado);
        Assert.NotNull(encontrado!.FechaRevocacion);
        Assert.Equal("nuevo-hash", encontrado.ReemplazadoPor);
    }

    [Fact]
    public async Task RefreshToken_RevocarTodosPorUsuario()
    {
        var repo = new RefreshTokenRepository(_dapper);
        var t1 = new RefreshToken
        {
            UsuarioId = 3,
            TokenHash = "hash-activo-1",
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
            FechaCreacion = DateTime.UtcNow
        };
        var t2 = new RefreshToken
        {
            UsuarioId = 3,
            TokenHash = "hash-activo-2",
            FechaExpiracion = DateTime.UtcNow.AddDays(14),
            FechaCreacion = DateTime.UtcNow
        };
        await repo.AgregarAsync(t1);
        await repo.AgregarAsync(t2);

        await repo.RevocarTodosPorUsuarioAsync(3);

        var r1 = await repo.ObtenerPorHashAsync("hash-activo-1");
        var r2 = await repo.ObtenerPorHashAsync("hash-activo-2");
        Assert.NotNull(r1!.FechaRevocacion);
        Assert.NotNull(r2!.FechaRevocacion);
    }
}
