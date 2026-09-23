using Microsoft.Data.SqlClient;

namespace ServiciosGenerales.Infraestructura.Data
{
    /// <summary>
    /// Wrapper liviano que expone la cadena de conexión y crea instancias
    /// <see cref="SqlConnection"/> para las consultas Dapper.
    ///
    /// Diseño:
    /// - Implementa <see cref="IDisposable"/> para que el caller pueda usar
    ///   <c>using var conn = _dapperContext.CrearConexion();</c>
    /// - Cada <see cref="CrearConexion"/> devuelve una conexión CERRADA; es
    ///   responsabilidad de Dapper abrirla y cerrarla con <c>Open()</c> o
    ///   usando el patrón <c>QueryAsync</c> que la abre automáticamente.
    /// </summary>
    public sealed class DapperContext : IDisposable
    {
        private readonly string _connectionString;

        /// <param name="connectionString">
        /// Misma cadena que se pasa a <c>DbContextOptions</c> de EF Core.
        /// </param>
        public DapperContext(string connectionString)
        {
            _connectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Crea una nueva <see cref="SqlConnection"/> CERRADA lista para
        /// usar con Dapper o <c>SqlCommand</c>.
        /// </summary>
        public SqlConnection CrearConexion() => new(_connectionString);

        /// <summary>
        /// Proporciona acceso a la cadena de conexión para construir
        /// <c>SqlCommand</c> o configuraciones de migración.
        /// </summary>
        public string ConnectionString => _connectionString;

        public void Dispose()
        {
            // No hay recursos que liberar explícitamente; las conexiones
            // individuales se cierran con 'using' en cada uso.
        }
    }
}
