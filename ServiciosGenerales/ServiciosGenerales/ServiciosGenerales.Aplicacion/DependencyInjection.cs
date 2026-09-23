using Microsoft.Extensions.DependencyInjection;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Aplicacion.UseCases.Auth;
using ServiciosGenerales.Aplicacion.UseCases.Parqueo;
using ServiciosGenerales.Aplicacion.UseCases.Porteria;
using ServiciosGenerales.Aplicacion.UseCases.Sistema;
using ServiciosGenerales.Aplicacion.UseCases.Usuarios;
using AdminUseCases = ServiciosGenerales.Aplicacion.UseCases.Admin;

namespace ServiciosGenerales.Aplicacion
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAplicacion(this IServiceCollection services)
        {
            services.AddSingleton<IServerClock, ServerClock>();
            services.AddScoped<IObtenerHoraServidorUseCase, ObtenerHoraServidorUseCase>();

            services.AddScoped<ILoginUseCase, LoginUseCase>();
            services.AddScoped<IRenovarTokenUseCase, RenovarTokenUseCase>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IRefreshTokenFactory, RefreshTokenFactory>();

            services.AddScoped<IBuscarOCrearUsuarioPorCarnetUseCase, BuscarOCrearUsuarioPorCarnetUseCase>();
            services.AddScoped<IObtenerUsuarioPorDniUseCase, ObtenerUsuarioPorDniUseCase>();
            services.AddScoped<IBuscarUsuariosCoincidentesUseCase, BuscarUsuariosCoincidentesUseCase>();
            services.AddScoped<ICrearUsuarioUseCase, CrearUsuarioUseCase>();

            services.AddScoped<IRegistrarEntradaUseCase, RegistrarEntradaUseCase>();
            services.AddScoped<IRegistrarSalidaUseCase, RegistrarSalidaUseCase>();
            services.AddScoped<IObtenerVisitasActivasUseCase, ObtenerVisitasActivasUseCase>();
            services.AddScoped<IRegistrarEntradaPorteriaCompletaUseCase, RegistrarEntradaPorteriaCompletaUseCase>();

            services.AddScoped<IRegistrarEntradaParqueoUseCase, RegistrarEntradaParqueoUseCase>();
            services.AddScoped<IRegistrarSalidaParqueoUseCase, RegistrarSalidaParqueoUseCase>();
            services.AddScoped<IObtenerParqueosActivosUseCase, ObtenerParqueosActivosUseCase>();
            services.AddScoped<IObtenerHistorialParqueoUseCase, ObtenerHistorialParqueoUseCase>();
            services.AddScoped<IBuscarVehiculoPorPlacaUseCase, BuscarVehiculoPorPlacaUseCase>();
            services.AddScoped<IListarVehiculosPorUsuarioUseCase, ListarVehiculosPorUsuarioUseCase>();
            services.AddScoped<IBuscarOCrearVehiculoUseCase, BuscarOCrearVehiculoUseCase>();
            services.AddScoped<IRegistrarEntradaParqueoCompletaUseCase, RegistrarEntradaParqueoCompletaUseCase>();

            services.AddScoped<AdminUseCases.IListarUsuariosUseCase, AdminUseCases.ListarUsuariosUseCase>();
            services.AddScoped<AdminUseCases.IObtenerHistorialPorteriaUseCase, AdminUseCases.ObtenerHistorialPorteriaUseCase>();
            services.AddScoped<AdminUseCases.IObtenerHistorialParqueoUseCase, AdminUseCases.ObtenerHistorialParqueoUseCase>();
            services.AddScoped<AdminUseCases.IActualizarUsuarioUseCase, AdminUseCases.ActualizarUsuarioUseCase>();
            services.AddScoped<AdminUseCases.ICambiarPasswordUsuarioUseCase, AdminUseCases.CambiarPasswordUsuarioUseCase>();
            services.AddScoped<AdminUseCases.IObtenerHistorialUsuarioUseCase, AdminUseCases.ObtenerHistorialUsuarioUseCase>();
            services.AddScoped<AdminUseCases.IObtenerEstadisticasTiposUseCase, AdminUseCases.ObtenerEstadisticasTiposUseCase>();
            services.AddScoped<AdminUseCases.IObtenerHistorialTipoUseCase, AdminUseCases.ObtenerHistorialTipoUseCase>();
            services.AddScoped<AdminUseCases.IObtenerAprobacionesOperadorUseCase, AdminUseCases.ObtenerAprobacionesOperadorUseCase>();

            return services;
        }
    }
}
