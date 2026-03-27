using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Marca
{
    public class CrearMarcaRequest
    {
        [Required(ErrorMessage = "El nombre de la marca es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre de la marca no puede superar los 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

       
    }
}