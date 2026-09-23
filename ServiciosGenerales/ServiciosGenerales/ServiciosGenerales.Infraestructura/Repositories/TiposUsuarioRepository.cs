using Dapper;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Infraestructura.Repositories
{
    /// <summary>
    /// Repositorio de tipos de usuario implementado con Dapper.
    /// Consulta simple contra la tabla TiposUsuario.
    /// </summary>
    public sealed class TiposUsuarioRepository : ITiposUsuarioRepository
    {
        private readonly DapperContext _dapper;

        public TiposUsuarioRepository(DapperContext dapper)
        {
            _dapper = dapper;
        }

        /// <summary>
        /// Obtiene todos los tipos de usuario ordenados por Id.
        /// Equivalente a: <c>SELECT Id, Nombre FROM TiposUsuario ORDER BY Id</c>.
        /// </summary>
        public async Task<IEnumerable<TiposUsuario>> ObtenerTodosAsync()
        {
            const string sql = "SELECT Id, Nombre FROM TiposUsuario ORDER BY Id";

            using var conn = _dapper.CrearConexion();
            return await conn.QueryAsync<TiposUsuario>(sql);
        }
    }
}
