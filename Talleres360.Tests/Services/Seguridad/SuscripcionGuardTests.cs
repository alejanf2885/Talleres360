using Moq;
using System;
using System.Threading.Tasks;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;
using Talleres360.Services.Seguridad;
using Talleres360.Services.Talleres;
using Xunit;

namespace Talleres360.Tests.Services.Seguridad
{
    public class SuscripcionGuardTests
    {
        [Fact]
        public async Task ValidarAccesoEscritura_DebeBloquear_CuandoTrialExpirado()
        {
            // 1. ARRANGE (Preparar)
            int tallerId = 1;
            Mock<ITallerRepository> mockRepo = new Mock<ITallerRepository>();

            Taller tallerExpirado = new Taller
            {
                Id = tallerId,
                TipoSuscripcion = "TRIAL",
                FechaCreacion = DateTime.UtcNow.AddDays(-31), // Simulamos pasado
                Activo = true
            };

            mockRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync(tallerExpirado);
            SuscripcionGuardService portero = new SuscripcionGuardService(mockRepo.Object);

            // 2. ACT (Ejecutar)
            (bool PuedeAcceder, string Mensaje) resultado = await portero.ValidarAccesoEscrituraAsync(tallerId);

            // 3. ASSERT (Verificar)
            Assert.False(resultado.PuedeAcceder);
            Assert.Contains("finalizado", resultado.Mensaje);
        }
    }
}