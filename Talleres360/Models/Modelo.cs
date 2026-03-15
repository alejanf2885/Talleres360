using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("MODELOS")]
    public class Modelo
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("MARCAID")]
        public int MarcaId { get; set; }

        [Column("TIPO_VEHICULO_ID")]
        public int TipoVehiculoId { get; set; }

        [Column("NOMBRE")]
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;
    }
}