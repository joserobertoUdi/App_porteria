namespace ServiciosGenerales.Aplicacion.Services
{
    public static class OffsetUtcUtil
    {
        /// <summary>
        /// Formatea un offset UTC como "+HH:mm" / "-HH:mm". Devuelve "Z" si es cero.
        /// </summary>
        public static string Formatear(TimeSpan offset)
        {
            if (offset == TimeSpan.Zero)
                return "Z";

            var signo = offset < TimeSpan.Zero ? "-" : "+";
            var absoluto = offset.Duration();
            return $"{signo}{absoluto.Hours:00}:{absoluto.Minutes:00}";
        }

        /// <summary>
        /// Compara dos offsets (dispositivo vs servidor) en minutos.
        /// </summary>
        public static bool EsMismoOffset(TimeSpan dispositivo, TimeSpan servidor)
        {
            return dispositivo.TotalMinutes == servidor.TotalMinutes;
        }
    }
}
