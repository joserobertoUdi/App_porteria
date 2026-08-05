using ServiciosGenerales.Aplicacion.Dtos.Usuarios;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Admin
{
    public interface IActualizarUsuarioUseCase
    {
        Task<bool> Ejecutar(int id, ActualizarUsuarioRequestDto dto);
    }

    /// <summary>
    /// Actualiza la información de una cuenta de sistema (nombre, tipo, rol y estado).
    /// Devuelve false si el usuario no existe (el controlador responde 404).
    /// </summary>
    public class ActualizarUsuarioUseCase : IActualizarUsuarioUseCase
    {
        private readonly IUsuarioRepository _repository;

        public ActualizarUsuarioUseCase(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Ejecutar(int id, ActualizarUsuarioRequestDto dto)
        {
            var usuario = await _repository.ObtenerPorIdAsync(id);
            if (usuario is null)
                return false;

            usuario.NombreCompleto = dto.NombreCompleto.Trim();
            usuario.TipoUsuarioId = dto.TipoUsuarioId;
            usuario.RolId = dto.RolId != 0 ? dto.RolId : 3;
            usuario.Estado = dto.Estado;

            return await _repository.ActualizarAsync(usuario);
        }
    }
}
