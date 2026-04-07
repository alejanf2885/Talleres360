using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("CategoriasProducto")]
    public class CategoriaProducto
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TallerId")]
        public int TallerId { get; set; }

        [Column("Nombre")]
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Column("Eliminado")]
        public bool Eliminado { get; set; }
    }
}
