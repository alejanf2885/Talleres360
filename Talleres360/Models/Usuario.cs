using Talleres360.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("USUARIOS")]
    public class Usuario
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TallerId")]
        public int? TallerId { get; set; } 

        [Column("Nombre")]
        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Column("Email")]
        [Required, StringLength(150)]
        public string Email { get; set; }

        [Column("Rol")]
        [Required]
        public RolesUsuario Rol { get; set; } 

        [Column("SecurityStamp")]
        [StringLength(255)]
        public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();

        [Column("Activo")]
        public bool Activo { get; set; } = true;

        [Column("Eliminado")]
        public bool Eliminado { get; set; } = false;

        [Column("FechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("Imagen")]
        public string? Imagen { get; set; }
    }
}