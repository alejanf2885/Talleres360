using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Modelo
{
    public class ActualizarModeloVehiculoDto
    {
        [Required(ErrorMessage = "La marca es obligatoria")]
        public int MarcaId { get; set; }

        [Required(ErrorMessage = "El tipo de vehículo es obligatorio")]
        public int VehiculoTipoId { get; set; }

        [Required(ErrorMessage = "El nombre del modelo es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre del modelo no puede superar los 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;
    }
}
