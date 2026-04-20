using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Servicios
{
    public class CrearServicioRequest
    {
        [Required(ErrorMessage = "El nombre del servicio es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres")]
        public string? Descripcion { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "El precio base no es válido")]
        public decimal PrecioBase { get; set; }

        [Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100")]
        public decimal ImpuestoPorcentaje { get; set; }

        public bool Activo { get; set; } = true;
    }
}
