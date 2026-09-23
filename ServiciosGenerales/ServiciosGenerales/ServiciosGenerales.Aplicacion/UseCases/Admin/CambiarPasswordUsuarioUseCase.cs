using ServiciosGenerales.Aplicacion.Dtos.Usuarios;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Admin
{
    public interface ICambiarPasswordUsuarioUseCase
    {
        Task<bool> Ejecutar(int id, CambiarPasswordRequestDto dto);
    }

    /// <summary>
    /// Restablece la contraseña de una cuenta (caso de contraseña olvidada) y de paso
    /// desbloquea la cuenta: limpia intentos fallidos y bloqueo temporal.
    /// Devuelve false si el usuario no existe (el controlador responde 404).
    /// </summary>
    public class CambiarPasswordUsuarioUseCase : ICambiarPasswordUsuarioUseCase
    {
        private readonly IUsuarioRepository _repository;

        public CambiarPasswordUsuarioUseCase(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Ejecutar(int id, CambiarPasswordRequestDto dto)
        {
            var usuario = await _repository.ObtenerPorIdAsync(id);
            if (usuario is null)
                return false;

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);
            usuario.IntentosFallidos = 0;
            usuario.BloqueoHasta = null;

            return await _repository.ActualizarAsync(usuario);
        }
    }
}
