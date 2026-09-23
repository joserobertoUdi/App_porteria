using Microsoft.EntityFrameworkCore;
using ServiciosGenerales.Dominio.Entidades;

namespace ServiciosGenerales.Infraestructura.Data
{
    public class UniversidadDbContext : DbContext
    {
        public UniversidadDbContext(DbContextOptions<UniversidadDbContext> options) : base(options) { }

        public DbSet<TiposUsuario> TiposUsuarios => Set<TiposUsuario>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<RegistroPorteria> RegistrosPorteria => Set<RegistroPorteria>();
        public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
        public DbSet<RegistroParqueo> RegistrosParqueo => Set<RegistroParqueo>();
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TiposUsuario>(entity =>
            {
                entity.ToTable("TiposUsuario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Nombre).IsUnique();

                entity.HasData(
                    new TiposUsuario { Id = 1, Nombre = "Estudiante" },
                    new TiposUsuario { Id = 2, Nombre = "Trabajador" },
                    new TiposUsuario { Id = 3, Nombre = "Visitante" }
                );
            });

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Nombre).IsUnique();

                entity.HasData(
                    new Rol { Id = 1, Nombre = "Administrador" },
                    new Rol { Id = 2, Nombre = "PorteroParqueo" },
                    new Rol { Id = 3, Nombre = "PorteroPorteria" }
                );
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DocumentoIdentidad).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.DocumentoIdentidad).IsUnique();
                entity.Property(e => e.FotoUrl).HasMaxLength(500);
                entity.Property(e => e.PasswordHash).HasMaxLength(500);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.Property(e => e.IntentosFallidos).HasDefaultValue(0);

                entity.HasOne(e => e.TipoUsuario)
                      .WithMany(t => t.Usuarios)
                      .HasForeignKey(e => e.TipoUsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Rol)
                      .WithMany(r => r.Usuarios)
                      .HasForeignKey(e => e.RolId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RegistroPorteria>(entity =>
            {
                entity.ToTable("RegistrosPorteria");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PuertaEntrada).HasMaxLength(50);
                entity.Property(e => e.PuertaSalida).HasMaxLength(50);
                entity.Property(e => e.MotivoVisita).HasMaxLength(500);
                entity.Property(e => e.AreaDestino).HasMaxLength(200);
                // Referencia 'sharepoint:{uid}': 45 caracteres hoy, con margen
                // por si el formato del identificador cambia.
                entity.Property(e => e.FotoUrl).HasMaxLength(200);
                entity.Property(e => e.FechaEntrada).HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.FechaSalida).HasFilter("[FechaSalida] IS NULL");
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_RegistrosPorteria_Fechas",
                    "[FechaSalida] IS NULL OR [FechaSalida] >= [FechaEntrada]"));

                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.RegistrosPorteria)
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RegistradoPor)
                      .WithMany()
                      .HasForeignKey(e => e.RegistradoPorUsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.RegistradoPorUsuarioId);
            });

            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.ToTable("Vehiculos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Matricula).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Matricula).IsUnique();
                entity.Property(e => e.Marca).HasMaxLength(50);
                entity.Property(e => e.Modelo).HasMaxLength(50);
                entity.Property(e => e.Color).HasMaxLength(30);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.Vehiculos)
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RegistroParqueo>(entity =>
            {
                entity.ToTable("RegistrosParqueo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PuertaAcceso).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Observaciones).HasMaxLength(255);
                entity.Property(e => e.FechaIngreso).HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.FechaSalida).HasFilter("[FechaSalida] IS NULL");
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_RegistrosParqueo_Fechas",
                    "[FechaSalida] IS NULL OR [FechaSalida] >= [FechaIngreso]"));

                entity.HasOne(e => e.Vehiculo)
                      .WithMany(v => v.RegistrosParqueo)
                      .HasForeignKey(e => e.VehiculoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RegistradoPor)
                      .WithMany()
                      .HasForeignKey(e => e.RegistradoPorUsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.RegistradoPorUsuarioId);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(64);
                entity.HasIndex(e => e.TokenHash).IsUnique();
                entity.Property(e => e.ReemplazadoPor).HasMaxLength(64);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_RefreshTokens_Fechas",
                    "[FechaExpiracion] > [FechaCreacion] AND ([FechaRevocacion] IS NULL OR [FechaRevocacion] >= [FechaCreacion])"));

                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
