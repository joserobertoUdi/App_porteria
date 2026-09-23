using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using ServiciosGenerales.Infraestructura.Data;
using Xunit;

namespace ServiciosGenerales.Tests;

public class ModeloIntegridadTests
{
    private static UniversidadDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<UniversidadDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ModeloIntegridadTests;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new UniversidadDbContext(options);
    }

    [Theory]
    [InlineData("Roles")]
    [InlineData("TiposUsuario")]
    public void Catalogos_TienenIndiceUnicoPorNombre(string tabla)
    {
        using var context = CrearContexto();
        var entity = context.GetService<IDesignTimeModel>().Model
            .GetEntityTypes().Single(e => e.GetTableName() == tabla);

        Assert.Contains(entity.GetIndexes(), indice =>
            indice.IsUnique && indice.Properties.Count == 1 && indice.Properties[0].Name == "Nombre");
    }

    [Theory]
    [InlineData("ServiciosGenerales.Dominio.Entidades.Usuario", "TipoUsuarioId")]
    [InlineData("ServiciosGenerales.Dominio.Entidades.Usuario", "RolId")]
    [InlineData("ServiciosGenerales.Dominio.Entidades.Vehiculo", "UsuarioId")]
    [InlineData("ServiciosGenerales.Dominio.Entidades.RegistroPorteria", "UsuarioId")]
    [InlineData("ServiciosGenerales.Dominio.Entidades.RegistroParqueo", "VehiculoId")]
    [InlineData("ServiciosGenerales.Dominio.Entidades.RefreshToken", "UsuarioId")]
    public void Relaciones_NoUsanBorradoEnCascada(string entidad, string propiedadForanea)
    {
        using var context = CrearContexto();
        var foreignKey = context.Model.FindEntityType(entidad)!
            .GetForeignKeys()
            .Single(fk => fk.Properties.Single().Name == propiedadForanea);

        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    [Theory]
    [InlineData("RegistrosPorteria", "CK_RegistrosPorteria_Fechas")]
    [InlineData("RegistrosParqueo", "CK_RegistrosParqueo_Fechas")]
    [InlineData("RefreshTokens", "CK_RefreshTokens_Fechas")]
    public void Movimientos_TienenRestriccionesDeFechas(string tabla, string restriccion)
    {
        using var context = CrearContexto();
        var entity = context.GetService<IDesignTimeModel>().Model
            .GetEntityTypes().Single(e => e.GetTableName() == tabla);

        Assert.Contains(entity.GetCheckConstraints(), check => check.Name == restriccion);
    }
}
