using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("VehiculoTipos")]
    public class VehiculoTipo
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Nombre")]
        [Required, StringLength(30)]
        public string Nombre { get; set; } = string.Empty;
    }
}