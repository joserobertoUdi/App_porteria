namespace ServiciosGenerales.Aplicacion.Dtos.Admin
{
    public class UsuarioListadoDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string DocumentoIdentidad { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class HistorialPorteriaDto
    {
        public int IdRegistro { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string DocumentoIdentidad { get; set; } = string.Empty;
        public DateTime FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }
        public string? PuertaEntrada { get; set; }
        public string? PuertaSalida { get; set; }
        public string? MotivoVisita { get; set; }
        public string? AreaDestino { get; set; }
    }

    public class HistorialParqueoDto
    {
        public int IdRegistro { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Color { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string DocumentoIdentidad { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaSalida { get; set; }
        public string PuertaAcceso { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }

    public class EstadisticaTipoDto
    {
        public int TipoUsuarioId { get; set; }
        public string TipoUsuario { get; set; } = string.Empty;
        public int PorteriaIngresos { get; set; }
        public int PorteriaSalidas { get; set; }
        public int ParqueoIngresos { get; set; }
        public int ParqueoSalidas { get; set; }
        public int TotalIngresos { get; set; }
        public int TotalSalidas { get; set; }
    }

    public class EstadisticasTiposResponseDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public List<EstadisticaTipoDto> Tipos { get; set; } = new();
    }

    public class HistorialTipoResponseDto
    {
        public int TipoUsuarioId { get; set; }
        public string TipoUsuario { get; set; } = string.Empty;
        public int Anio { get; set; }
        public int Mes { get; set; }
        public List<HistorialPorteriaDto> Porteria { get; set; } = new();
        public List<HistorialParqueoDto> Parqueo { get; set; } = new();
    }
}
