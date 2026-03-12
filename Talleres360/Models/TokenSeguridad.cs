using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("TOKENS_SEGURIDAD")]
    public class TokenSeguridad
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("USUARIOID")]
        public int UsuarioId { get; set; }

        [Column("TOKEN")]
        public string Token { get; set; }

        [Column("TIPOTOKEN")]
        public string TipoToken { get; set; }

        [Column("FECHACREACION")]
        public DateTime FechaCreacion { get; set; }

        [Column("FECHAEXPIRACION")]
        public DateTime FechaExpiracion { get; set; }

        [Column("USADO")]
        public bool Usado { get; set; }

    }

}
