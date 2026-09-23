using ServiciosGenerales.Aplicacion.Dtos.Usuarios;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Usuarios
{
    public interface IBuscarUsuariosCoincidentesUseCase
    {
        Task<IEnumerable<UsuarioDto>> Ejecutar(string termino);
    }

    public class BuscarUsuariosCoincidentesUseCase : IBuscarUsuariosCoincidentesUseCase
    {
        private readonly IUsuarioRepository _repository;

        public BuscarUsuariosCoincidentesUseCase(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UsuarioDto>> Ejecutar(string termino)
        {
            var usuarios = await _repository.ObtenerCoincidenciaAsync(termino);

            return usuarios.Select(u => new UsuarioDto
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                DocumentoIdentidad = u.DocumentoIdentidad,
                TipoUsuarioId = u.TipoUsuarioId,
                FotoUrl = u.FotoUrl,
            });
        }
    }
}
