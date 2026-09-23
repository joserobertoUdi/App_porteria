namespace ServiciosGenerales.Dominio.Entidades
{
    public class Rol
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
