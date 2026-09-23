using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;
using ServiciosGenerales.Infraestructura.Repositories;
using ServiciosGenerales.Infraestructura.Services;

namespace ServiciosGenerales.Infraestructura
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Registra los servicios de infraestructura en el contenedor DI.
        ///
        /// <para><b>EF Core</b> se mantiene exclusivamente para migraciones
        /// y seed de datos. Los repositorios ahora usan Dapper vía
        /// <see cref="DapperContext"/>.</para>
        /// </summary>
        public static IServiceCollection AddInfraestructura(this IServiceCollection services, string connectionString)
        {
            // ── EF Core: solo para migraciones y seed ──
            services.AddDbContext<UniversidadDbContext>(options =>
                options.UseSqlServer(connectionString));

            // ── Dapper: repositorios de lectura/escritura ──
            services.AddSingleton(new DapperContext(connectionString));

            // ── Repositorios (Dapper) ──
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IRegistroPorteriaRepository, RegistroPorteriaRepository>();
            services.AddScoped<IVehiculoRepository, VehiculoRepository>();
            services.AddScoped<IRegistroParqueoRepository, RegistroParqueoRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ITiposUsuarioRepository, TiposUsuarioRepository>();

            // ── Servicio documental (SharepointApi) ──
            services.AddHttpClient<ISharepointService, SharepointService>();

            return services;
        }
    }
}
