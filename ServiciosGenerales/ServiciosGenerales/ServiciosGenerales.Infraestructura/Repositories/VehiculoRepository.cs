using Dapper;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Infraestructura.Repositories
{
    /// <summary>
    /// Repositorio de vehículos implementado con Dapper.
    /// Consultas contra la tabla Vehiculos con JOIN a Usuarios.
    /// </summary>
    public sealed class VehiculoRepository : IVehiculoRepository
    {
        private readonly DapperContext _dapper;

        public VehiculoRepository(DapperContext dapper)
        {
            _dapper = dapper;
        }

        /// <summary>
        /// Obtiene un vehículo por su Id.
        /// </summary>
        public async Task<Vehiculo?> ObtenerPorIdAsync(int id)
        {
            const string sql = """
                SELECT Id, UsuarioId, Matricula, Marca, Modelo, Color, Activo, FechaRegistro
                FROM Vehiculos
                WHERE Id = @Id
                """;

            using var conn = _dapper.CrearConexion();
            return await conn.QuerySingleOrDefaultAsync<Vehiculo>(sql, new { Id = id });
        }

        /// <summary>
        /// Busca un vehículo por matrícula incluyendo el usuario dueño.
        /// </summary>
        public async Task<Vehiculo?> ObtenerPorMatriculaAsync(string matricula)
        {
            const string sql = """
                SELECT
                    v.Id, v.UsuarioId, v.Matricula, v.Marca, v.Modelo, v.Color, v.Activo, v.FechaRegistro,
                    u.Id, u.NombreCompleto, u.DocumentoIdentidad, u.TipoUsuarioId,
                    u.FotoUrl, u.PasswordHash, u.RolId, u.FechaRegistro, u.Estado,
                    u.IntentosFallidos, u.BloqueoHasta
                FROM Vehiculos v
                INNER JOIN Usuarios u ON v.UsuarioId = u.Id
                WHERE v.Matricula = @Matricula
                """;

            using var conn = _dapper.CrearConexion();

            var resultado = await conn.QueryAsync<Vehiculo, Usuario, Vehiculo>(
                sql,
                (vehiculo, usuario) =>
                {
                    vehiculo.Usuario = usuario;
                    return vehiculo;
                },
                new { Matricula = matricula },
                splitOn: "Id,Id");

            return resultado.FirstOrDefault();
        }

        /// <summary>
        /// Obtiene todos los vehículos de un usuario incluyendo la info del usuario.
        /// </summary>
        public async Task<IEnumerable<Vehiculo>> ObtenerPorUsuarioIdAsync(int usuarioId)
        {
            const string sql = """
                SELECT
                    v.Id, v.UsuarioId, v.Matricula, v.Marca, v.Modelo, v.Color, v.Activo, v.FechaRegistro,
                    u.Id, u.NombreCompleto, u.DocumentoIdentidad, u.TipoUsuarioId,
                    u.FotoUrl, u.PasswordHash, u.RolId, u.FechaRegistro, u.Estado,
                    u.IntentosFallidos, u.BloqueoHasta
                FROM Vehiculos v
                INNER JOIN Usuarios u ON v.UsuarioId = u.Id
                WHERE v.UsuarioId = @UsuarioId
                """;

            using var conn = _dapper.CrearConexion();

            var resultado = await conn.QueryAsync<Vehiculo, Usuario, Vehiculo>(
                sql,
                (vehiculo, usuario) =>
                {
                    vehiculo.Usuario = usuario;
                    return vehiculo;
                },
                new { UsuarioId = usuarioId },
                splitOn: "Id,Id");

            return resultado;
        }

        /// <summary>
        /// Crea un vehículo y devuelve su Id.
        /// </summary>
        public async Task<int> CrearAsync(Vehiculo vehiculo)
        {
            const string sql = """
                INSERT INTO Vehiculos
                    (UsuarioId, Matricula, Marca, Modelo, Color, Activo, FechaRegistro)
                VALUES
                    (@UsuarioId, @Matricula, @Marca, @Modelo, @Color, @Activo, @FechaRegistro);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """;

            using var conn = _dapper.CrearConexion();
            await conn.OpenAsync();

            var id = await conn.ExecuteScalarAsync<int>(sql, new
            {
                vehiculo.UsuarioId,
                vehiculo.Matricula,
                vehiculo.Marca,
                vehiculo.Modelo,
                vehiculo.Color,
                vehiculo.Activo,
                vehiculo.FechaRegistro
            });

            vehiculo.Id = id;
            return id;
        }

        /// <summary>
        /// Actualiza un vehículo existente.
        /// </summary>
        public async Task<bool> ActualizarAsync(Vehiculo vehiculo)
        {
            const string sql = """
                UPDATE Vehiculos
                SET Matricula = @Matricula,
                    Marca = @Marca,
                    Modelo = @Modelo,
                    Color = @Color,
                    Activo = @Activo
                WHERE Id = @Id
                """;

            using var conn = _dapper.CrearConexion();
            var filas = await conn.ExecuteAsync(sql, new
            {
                vehiculo.Matricula,
                vehiculo.Marca,
                vehiculo.Modelo,
                vehiculo.Color,
                vehiculo.Activo,
                vehiculo.Id
            });

            return filas > 0;
        }
    }
}
