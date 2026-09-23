namespace ServiciosGenerales.Dominio.Entidades
{
    public class TiposUsuario
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
