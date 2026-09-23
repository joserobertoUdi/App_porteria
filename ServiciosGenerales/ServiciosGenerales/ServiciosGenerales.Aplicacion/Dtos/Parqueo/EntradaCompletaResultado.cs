namespace ServiciosGenerales.Aplicacion.Dtos.Parqueo
{
    public class EntradaCompletaResultado
    {
        public int RegistroId { get; set; }
        public int UsuarioId { get; set; }
        public int VehiculoId { get; set; }
        public bool UsuarioCreado { get; set; }
        public bool VehiculoCreado { get; set; }
    }
}
