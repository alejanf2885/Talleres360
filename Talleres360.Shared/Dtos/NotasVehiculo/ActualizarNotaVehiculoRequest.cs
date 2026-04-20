using System.ComponentModel.DataAnnotations;
using Talleres360.Enums;

namespace Talleres360.Dtos.NotasVehiculo
{
    public class ActualizarNotaVehiculoRequest
    {
        [Required(ErrorMessage = "El texto es obligatorio")]
        [StringLength(1000, ErrorMessage = "El texto no puede superar 1000 caracteres")]
        public string Texto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de nota es obligatorio")]
        public NotaVehiculoTipo? Tipo { get; set; }
    }
}
