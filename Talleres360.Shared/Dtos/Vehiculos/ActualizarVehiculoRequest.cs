using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Vehiculos
{
    public class ActualizarVehiculoRequest
    {
        public int? ClienteId { get; set; }

        [Required(ErrorMessage = "El tipo de vehículo es obligatorio")]
        public int VehiculoTipoId { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        public int MarcaId { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio")]
        public int ModeloId { get; set; }

        [Required(ErrorMessage = "La matrícula es obligatoria")]
        [StringLength(15, ErrorMessage = "La matrícula no puede superar 15 caracteres")]
        public string Matricula { get; set; } = string.Empty;

        public int? Anio { get; set; }
        public int? KmActuales { get; set; }
        public decimal? PromedioKmDiarios { get; set; }
    }
}
