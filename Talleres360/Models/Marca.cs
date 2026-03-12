using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("MARCAS")]
    public class Marca
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NOMBRE")]
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}