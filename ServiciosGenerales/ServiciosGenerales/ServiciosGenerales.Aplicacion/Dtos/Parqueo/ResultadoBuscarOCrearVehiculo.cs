using ServiciosGenerales.Dominio.Entidades;

namespace ServiciosGenerales.Aplicacion.Dtos.Parqueo
{
    public class ResultadoBuscarOCrearVehiculo
    {
        public required Vehiculo Vehiculo { get; set; }
        public bool Creado { get; set; }
    }
}
