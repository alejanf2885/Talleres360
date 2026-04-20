using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Citas
{
    public class ConvertirCitaTrabajoRequest
    {
        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje de entrada no es válido")]
        public int KmEntrada { get; set; }

        [StringLength(150, ErrorMessage = "El título no puede superar 150 caracteres")]
        public string? TituloMantenimiento { get; set; }

        [StringLength(50, ErrorMessage = "El número de documento no puede superar 50 caracteres")]
        public string? NumeroDocumento { get; set; }

        public int? MecanicoAsignadoId { get; set; }
    }
}
