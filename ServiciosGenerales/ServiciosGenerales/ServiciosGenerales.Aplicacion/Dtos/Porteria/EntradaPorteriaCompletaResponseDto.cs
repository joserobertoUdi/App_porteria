namespace ServiciosGenerales.Aplicacion.Dtos.Porteria
{
    public class EntradaPorteriaCompletaResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public bool UsuarioCreado { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
