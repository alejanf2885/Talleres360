using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Inventario
{
    public class CrearCategoriaProductoRequest
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;
    }
}
