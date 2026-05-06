using System.ComponentModel.DataAnnotations;

namespace Talleres360_front.Models.Inventario;

public class CategoriaFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Máximo 255 caracteres")]
    public string? Descripcion { get; set; }
}
