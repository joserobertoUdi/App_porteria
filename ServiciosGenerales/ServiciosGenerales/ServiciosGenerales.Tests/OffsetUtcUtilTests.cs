using ServiciosGenerales.Aplicacion.Services;
using Xunit;

namespace ServiciosGenerales.Tests
{
    public class OffsetUtcUtilTests
    {
        [Theory]
        [InlineData(0, "Z")]
        [InlineData(-300, "-05:00")]
        [InlineData(300, "+05:00")]
        [InlineData(330, "+05:30")]
        [InlineData(-210, "-03:30")]
        [InlineData(75, "+01:15")]
        public void Formatear_DevuelveFormatoEsperado(int totalMinutos, string esperado)
        {
            var offset = TimeSpan.FromMinutes(totalMinutos);
            Assert.Equal(esperado, OffsetUtcUtil.Formatear(offset));
        }

        [Theory]
        [InlineData(0, 0, 0, 0, true)]
        [InlineData(-5, 0, -5, 0, true)]
        [InlineData(-5, 0, -6, 0, false)]
        [InlineData(1, 0, -1, 0, false)]
        public void EsMismoOffset_ComparaEnMinutos(int hDisp, int mDisp, int hServ, int mServ, bool esperado)
        {
            var dispositivo = new TimeSpan(hDisp, mDisp, 0);
            var servidor = new TimeSpan(hServ, mServ, 0);
            Assert.Equal(esperado, OffsetUtcUtil.EsMismoOffset(dispositivo, servidor));
        }
    }
}
