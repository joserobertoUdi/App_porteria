namespace ServiciosGenerales.Aplicacion.Dtos.Parqueo
{
    public class EntradaParqueoCompletaResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int VehiculoId { get; set; }
        public bool UsuarioCreado { get; set; }
        public bool VehiculoCreado { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
