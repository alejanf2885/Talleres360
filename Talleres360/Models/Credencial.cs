using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("CREDENCIALES")]
    public class Credencial
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Column("TipoInicioSesion")]
        public string TipoInicioSesion { get; set; } = "LOCAL";

        [Column("ProviderKey")]
        public string? ProviderKey { get; set; }

        [Column("PasswordHash")]
        public string? PasswordHash { get; set; }

        [Column("FechaUltimoAcceso")]
        public DateTime? FechaUltimoAcceso { get; set; }

        [Column("Eliminado")]
        public bool Eliminado { get; set; } = false;
    }
}