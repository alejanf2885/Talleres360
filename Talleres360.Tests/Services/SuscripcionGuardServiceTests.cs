using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Talleres360.Dtos.Seguridad;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;
using Talleres360.Services.Talleres;

namespace Talleres360.Tests.Services
{
    public class SuscripcionGuardServiceTests
    {
        private readonly Mock<ITallerRepository> _tallerRepoMock;
        
        private static IConfiguration BuildConfig(bool stripeEnabled)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Stripe:Enabled", stripeEnabled.ToString().ToLower() }
                })
                .Build();
        }

        public SuscripcionGuardServiceTests()
        {
            _tallerRepoMock = new Mock<ITallerRepository>();
        }

        [Fact]
        public async Task ValidarAccesoEscrituraAsync_StripeDesactivado_SiemprePermitido()
        {
            // Arrange
            var sut = new SuscripcionGuardService(
                _tallerRepoMock.Object,
                BuildConfig(false)
            );

            // Act
            var result = await sut.ValidarAccesoEscrituraAsync(1);

            // Assert
            Assert.True(result.PuedeAcceder);
            _tallerRepoMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ValidarAccesoEscrituraAsync_TallerNoEncontrado_Denegado()
        {
            // Arrange
            var sut = new SuscripcionGuardService(
                _tallerRepoMock.Object,
                BuildConfig(true)
            );
            _tallerRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Taller)null!);

            // Act
            var result = await sut.ValidarAccesoEscrituraAsync(1);

            // Assert
            Assert.False(result.PuedeAcceder);
            Assert.Equal(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarAccesoEscrituraAsync_TrialExpirado_Denegado()
        {
            // Arrange
            var sut = new SuscripcionGuardService(
                _tallerRepoMock.Object,
                BuildConfig(true)
            );
            var taller = new Taller 
            { 
                Id = 1, 
                TipoSuscripcion = "TRIAL", 
                FechaCreacion = DateTime.UtcNow.AddDays(-31), // Create 31 days ago, expired
                Activo = true 
            };
            _tallerRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(taller);

            // Act
            var result = await sut.ValidarAccesoEscrituraAsync(1);

            // Assert
            Assert.False(result.PuedeAcceder);
            Assert.Equal(ErrorCode.SUBS_SIN_PLAN_ACTIVO.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ValidarAccesoEscrituraAsync_TrialVigente_Permitido()
        {
            // Arrange
            var sut = new SuscripcionGuardService(
                _tallerRepoMock.Object,
                BuildConfig(true)
            );
            var taller = new Taller 
            { 
                Id = 1, 
                TipoSuscripcion = "TRIAL", 
                FechaCreacion = DateTime.UtcNow.AddDays(-10), // Created 10 days ago, 20 days left
                Activo = true 
            };
            _tallerRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(taller);

            // Act
            var result = await sut.ValidarAccesoEscrituraAsync(1);

            // Assert
            Assert.True(result.PuedeAcceder);
        }

        [Fact]
        public async Task ValidarAccesoEscrituraAsync_TallerInactivo_Denegado()
        {
            // Arrange
            var sut = new SuscripcionGuardService(
                _tallerRepoMock.Object,
                BuildConfig(true)
            );
            var taller = new Taller 
            { 
                Id = 1, 
                TipoSuscripcion = "PRO", 
                Activo = false 
            };
            _tallerRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(taller);

            // Act
            var result = await sut.ValidarAccesoEscrituraAsync(1);

            // Assert
            Assert.False(result.PuedeAcceder);
            Assert.Equal(ErrorCode.AUTH_CUENTA_INACTIVA.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ObtenerEstadoSuscripcionAsync_StripeDesactivado_BETA()
        {
            // Arrange
            var sut = new SuscripcionGuardService(
                _tallerRepoMock.Object,
                BuildConfig(false)
            );

            // Act
            var result = await sut.ObtenerEstadoSuscripcionAsync(1);

            // Assert
            Assert.True(result.EsActivo);
            Assert.Equal("BETA", result.TipoSuscripcion);
        }

        [Fact]
        public async Task ObtenerEstadoSuscripcionAsync_TrialConDiasRestantes()
        {
            // Arrange
            var sut = new SuscripcionGuardService(
                _tallerRepoMock.Object,
                BuildConfig(true)
            );
            var taller = new Taller 
            { 
                Id = 1, 
                TipoSuscripcion = "TRIAL", 
                FechaCreacion = DateTime.UtcNow.AddDays(-20), // 10 days left
                Activo = true 
            };
            _tallerRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(taller);

            // Act
            var result = await sut.ObtenerEstadoSuscripcionAsync(1);

            // Assert
            Assert.True(result.EsActivo);
            Assert.Equal("TRIAL", result.TipoSuscripcion);
            Assert.InRange(result.DiasRestantesTrial, 9, 10);
        }

        [Fact]
        public async Task ObtenerEstadoSuscripcionAsync_TrialExpirado_EsActivoFalse()
        {
            // Arrange
            var sut = new SuscripcionGuardService(
                _tallerRepoMock.Object,
                BuildConfig(true)
            );
            var taller = new Taller 
            { 
                Id = 1, 
                TipoSuscripcion = "TRIAL", 
                FechaCreacion = DateTime.UtcNow.AddDays(-31), // -1 day left
                Activo = true 
            };
            _tallerRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(taller);

            // Act
            var result = await sut.ObtenerEstadoSuscripcionAsync(1);

            // Assert
            Assert.False(result.EsActivo);
            Assert.Equal(0, result.DiasRestantesTrial);
            Assert.Equal("TRIAL", result.TipoSuscripcion);
            Assert.Contains("finalizado", result.Mensaje);
        }
    }
}