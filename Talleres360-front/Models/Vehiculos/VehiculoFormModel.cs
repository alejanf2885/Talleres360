using System.ComponentModel.DataAnnotations;

namespace Talleres360_front.Models.Vehiculos;

public class VehiculoFormModel
{
    public int Id { get; set; }

    public int? ClienteId { get; set; }

    [Required(ErrorMessage = "El tipo de vehículo es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un tipo de vehículo")]
    public int VehiculoTipoId { get; set; }

    [Required(ErrorMessage = "La marca es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una marca")]
    public int MarcaId { get; set; }

    [Required(ErrorMessage = "El modelo es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un modelo")]
    public int ModeloId { get; set; }

    [Required(ErrorMessage = "La matrícula es obligatoria")]
    [StringLength(15, ErrorMessage = "Máximo 15 caracteres")]
    public string Matricula { get; set; } = string.Empty;

    [Range(1900, 2100, ErrorMessage = "Año inválido")]
    public int? Anio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Los km no pueden ser negativos")]
    public int? KmActuales { get; set; }
}
