using ServiciosGenerales.Aplicacion.Dtos.Usuarios;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Usuarios
{
    public interface IBuscarOCrearUsuarioPorCarnetUseCase
    {
        Task<ResultadoBuscarOCrearUsuario> Ejecutar(string documentoIdentidad, string? nombreCompleto, int tipoUsuarioId);
    }

    /// <summary>
    /// Busca un usuario por documento de identidad; si no existe, lo crea SIN credenciales
    /// (PasswordHash null) y SIN rol de sistema (RolId null), de modo que NO pueda iniciar
    /// sesión en la aplicación. El TipoUsuario refleja su identidad (Estudiante/Trabajador/
    /// Visitante), mientras que el rol de sistema es exclusivo de cuentas que acceden a la app.
    /// </summary>
    public class BuscarOCrearUsuarioPorCarnetUseCase : IBuscarOCrearUsuarioPorCarnetUseCase
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public BuscarOCrearUsuarioPorCarnetUseCase(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<ResultadoBuscarOCrearUsuario> Ejecutar(string documentoIdentidad, string? nombreCompleto, int tipoUsuarioId)
        {
            var documento = documentoIdentidad.Trim();
            var usuario = await _usuarioRepository.ObtenerPorDocumentoAsync(documento);

            if (usuario != null)
            {
                return new ResultadoBuscarOCrearUsuario { Usuario = usuario, Creado = false };
            }

            usuario = new Usuario
            {
                NombreCompleto = string.IsNullOrWhiteSpace(nombreCompleto) ? documento : nombreCompleto.Trim(),
                DocumentoIdentidad = documento,
                TipoUsuarioId = tipoUsuarioId is >= 1 and <= 3 ? tipoUsuarioId : 3,
                RolId = null,
                PasswordHash = null,
                Estado = true,
            };

            await _usuarioRepository.CrearAsync(usuario);
            return new ResultadoBuscarOCrearUsuario { Usuario = usuario, Creado = true };
        }
    }
}
