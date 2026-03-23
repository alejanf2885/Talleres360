using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("TokensSeguridad")]  
    public class TokenSeguridad
    {
        [Key]
        [Column("Id")]  
        public int Id { get; set; }

        [Column("UsuarioId")] 
        public int? UsuarioId { get; set; }  

        [Column("Token")]  
        [Required, StringLength(255)]
        public string Token { get; set; } = string.Empty;

        [Column("TipoToken")]  
        [Required, StringLength(50)]
        public string TipoToken { get; set; } = "RESET_PASSWORD";

        [Column("FechaCreacion")] 
        public DateTime FechaCreacion { get; set; }

        [Column("FechaExpiracion")] 
        public DateTime FechaExpiracion { get; set; }

        [Column("Usado")]  
        public bool Usado { get; set; } = false;
    }
}