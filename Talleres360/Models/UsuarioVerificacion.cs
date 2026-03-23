using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("UsuarioVerificaciones")]
    public class UsuarioVerificacion
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Token")]
        public string Token { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Column("Tipo")]
        public string Tipo { get; set; } = null!;

        [Required]
        [Column("FechaExpiracion")]
        public DateTime FechaExpiracion { get; set; }

        [Required]
        [Column("FechaCreacion")]
        public DateTime FechaCreacion { get; set; }
    }
}