namespace ServiciosGenerales.Aplicacion.Services
{
    /// <summary>
    /// Fuente única de la hora del servidor para todo el negocio.
    /// Permite que las pruebas unitarias inyecten una hora fija.
    /// </summary>
    public interface IServerClock
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }

    public class ServerClock : IServerClock
    {
        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
