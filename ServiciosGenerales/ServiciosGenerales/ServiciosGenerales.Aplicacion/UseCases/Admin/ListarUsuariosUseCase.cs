using ServiciosGenerales.Aplicacion.Dtos.Admin;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Admin
{
    public interface IListarUsuariosUseCase
    {
        Task<IEnumerable<UsuarioListadoDto>> Ejecutar();
    }

    /// <summary>
    /// Lista las cuentas de sistema (usuarios con rol de sistema). Solo incluye:
    /// Administrador, Portero Parqueo y Portero Portería. Excluye a los usuarios creados
    /// automáticamente por los flujos de entrada, que no tienen rol de sistema ni credenciales.
    /// </summary>
    public class ListarUsuariosUseCase : IListarUsuariosUseCase
    {
        private static readonly int[] RolesDeSistema = { 1, 2, 3 };

        private readonly IUsuarioRepository _repository;

        public ListarUsuariosUseCase(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UsuarioListadoDto>> Ejecutar()
        {
            var usuarios = await _repository.ObtenerTodosAsync();

            return usuarios
                .Where(u => u.RolId.HasValue && RolesDeSistema.Contains(u.RolId.Value))
                .Select(u => new UsuarioListadoDto
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                DocumentoIdentidad = u.DocumentoIdentidad,
                TipoUsuario = u.TipoUsuario?.Nombre ?? "",
                Rol = u.Rol?.Nombre ?? "",
                Estado = u.Estado,
                FechaRegistro = u.FechaRegistro,
            });
        }
    }
}
