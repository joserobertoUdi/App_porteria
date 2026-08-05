using ServiciosGenerales.Aplicacion.Dtos.Usuarios;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Usuarios
{
    public interface IObtenerUsuarioPorDniUseCase
    {
        Task<UsuarioDto?> Ejecutar(string dni);
    }

    public class ObtenerUsuarioPorDniUseCase : IObtenerUsuarioPorDniUseCase
    {
        private readonly IUsuarioRepository _repository;

        public ObtenerUsuarioPorDniUseCase(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<UsuarioDto?> Ejecutar(string dni)
        {
            var usuario = await _repository.ObtenerPorDocumentoAsync(dni);
            if (usuario == null)
                return null;

            return new UsuarioDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                DocumentoIdentidad = usuario.DocumentoIdentidad,
                TipoUsuarioId = usuario.TipoUsuarioId,
                FotoUrl = usuario.FotoUrl,
            };
        }
    }
}
