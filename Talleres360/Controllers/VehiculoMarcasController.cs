using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Talleres360.API.Filters;
using Talleres360.Dtos.Marca;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;
using Talleres360.Repositories.Vehiculos;

namespace Talleres360.Controllers
{
    [Route("api/v1/vehiculos/marcas")]
    [ApiController]
    [Authorize]
    public class VehiculoMarcasController : ControllerBase
    {
        private readonly IMarcaService _marcaService;
        private readonly IUserContextService _userContextService;

        public VehiculoMarcasController(
            IMarcaService marcaService,
            IUserContextService userContextService)
        {
            ArgumentNullException.ThrowIfNull(marcaService);
            ArgumentNullException.ThrowIfNull(userContextService);

            _marcaService = marcaService;
            _userContextService = userContextService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<List<MarcaVehiculoDto>> resultado = await _marcaService.ObtenerMarcasAsync(tallerId.Value);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al recuperar marcas."));
            }

            List<MarcaVehiculoDto> marcas = resultado.Data ?? new List<MarcaVehiculoDto>();

            return Ok(ApiResponse<List<MarcaVehiculoDto>>.Ok(
                marcas,
                "Marcas recuperadas correctamente."));
        }

        [HttpPost]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Create([FromBody] CrearMarcaRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            bool esOficial = false;
            string? rol = _userContextService.GetRol();

            
            if (rol != null && rol.Equals("SUPERADMIN", StringComparison.OrdinalIgnoreCase))
            {
                esOficial = true;
            }


            ServiceResult<MarcaVehiculoDto> resultado = await _marcaService
                .RegistrarMarcaAsync(tallerId.Value, request.Nombre, esOficial);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al registrar la marca."));
            }

            MarcaVehiculoDto marca = resultado.Data ?? new MarcaVehiculoDto();


            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<MarcaVehiculoDto>.Ok(marca, "¡Marca registrada con éxito!"));
        }


        [HttpDelete("{id:int:min(1)}")]
        [TallerAuthorize<IMarcaRepository>]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Delete(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }
            ServiceResult<bool> resultado = await _marcaService.EliminarMarcaAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al eliminar la marca."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "¡Marca eliminada con éxito!"));


        }
    }
}
