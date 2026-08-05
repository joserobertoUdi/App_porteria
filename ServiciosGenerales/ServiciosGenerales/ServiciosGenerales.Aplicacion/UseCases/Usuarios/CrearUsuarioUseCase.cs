using ServiciosGenerales.Aplicacion.Dtos.Usuarios;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Usuarios
{
    public interface ICrearUsuarioUseCase
    {
        Task<int> Ejecutar(CrearUsuarioRequestDto dto);
    }

    public class CrearUsuarioUseCase : ICrearUsuarioUseCase
    {
        private readonly IUsuarioRepository _repository;

        public CrearUsuarioUseCase(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Ejecutar(CrearUsuarioRequestDto dto)
        {
            var documento = dto.DocumentoIdentidad.Trim();
            var existente = await _repository.ObtenerPorDocumentoAsync(documento);
            if (existente != null)
                throw new InvalidOperationException("Ya existe un usuario con ese documento de identidad.");

            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto.Trim(),
                DocumentoIdentidad = documento,
                TipoUsuarioId = dto.TipoUsuarioId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RolId = dto.RolId != 0 ? dto.RolId : 3,
                Estado = true,
            };

            return await _repository.CrearAsync(usuario);
        }
    }
}
