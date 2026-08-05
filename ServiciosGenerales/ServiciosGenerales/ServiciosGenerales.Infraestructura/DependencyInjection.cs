using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;
using ServiciosGenerales.Infraestructura.Repositories;

namespace ServiciosGenerales.Infraestructura
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfraestructura(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<UniversidadDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IRegistroPorteriaRepository, RegistroPorteriaRepository>();
            services.AddScoped<IVehiculoRepository, VehiculoRepository>();
            services.AddScoped<IRegistroParqueoRepository, RegistroParqueoRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ITiposUsuarioRepository, TiposUsuarioRepository>();

            return services;
        }
    }
}
