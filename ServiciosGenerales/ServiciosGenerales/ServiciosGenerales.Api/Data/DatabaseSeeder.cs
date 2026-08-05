using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Api.Data
{
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Crea el usuario administrador y, si está habilitado (solo desarrollo),
        /// usuarios de demostración. Las credenciales provienen de variables de entorno
        /// o de la configuración; nunca se hardcodean contraseñas de producción.
        /// </summary>
        public static async Task SeedAsync(UniversidadDbContext context, IConfiguration configuration)
        {
            if (context.Usuarios.Any())
                return;

            var usuarios = new List<Dominio.Entidades.Usuario>();

            var adminPassword = configuration.GetValue<string>("SEED_ADMIN_PASSWORD");
            if (!string.IsNullOrWhiteSpace(adminPassword))
            {
                usuarios.Add(new Dominio.Entidades.Usuario
                {
                    NombreCompleto = configuration.GetValue<string>("SEED_ADMIN_NOMBRE") ?? "Administrador",
                    DocumentoIdentidad = configuration.GetValue<string>("SEED_ADMIN_DOCUMENTO") ?? "admin",
                    TipoUsuarioId = configuration.GetValue<int?>("SEED_ADMIN_TIPO_USUARIO_ID") ?? 1,
                    RolId = configuration.GetValue<int?>("SEED_ADMIN_ROL_ID") ?? 1,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Estado = true,
                });
            }

            var usuariosDemo = configuration.GetValue<bool?>("SEED_USUARIOS_DEMO") ?? false;
            if (usuariosDemo)
            {
                usuarios.Add(new Dominio.Entidades.Usuario
                {
                    NombreCompleto = "Portero Parqueo",
                    DocumentoIdentidad = "portero.parqueo",
                    TipoUsuarioId = 2,
                    RolId = 2,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                        configuration.GetValue<string>("SEED_PORTERO_PARQUEO_PASSWORD") ?? "portero123"),
                    Estado = true,
                });
                usuarios.Add(new Dominio.Entidades.Usuario
                {
                    NombreCompleto = "Portero Portería",
                    DocumentoIdentidad = "portero.porteria",
                    TipoUsuarioId = 2,
                    RolId = 3,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                        configuration.GetValue<string>("SEED_PORTERO_PORTERIA_PASSWORD") ?? "portero123"),
                    Estado = true,
                });
            }

            if (usuarios.Count > 0)
            {
                context.Usuarios.AddRange(usuarios);
                await context.SaveChangesAsync();
            }
        }
    }
}
