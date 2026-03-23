using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("Modelos")]  
    public class Modelo
    {
        [Key]
        [Column("Id")]  
        public int Id { get; set; }

        [Column("MarcaId")]  
        public int MarcaId { get; set; }

        [Column("VehiculoTipoId")]  
        public int VehiculoTipoId { get; set; }

        [Column("Nombre")]  
        [Required, StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Column("TallerId")]
        public int? TallerId { get; set; }

        [Column("EsOficial")]
        public bool EsOficial { get; set; } = false;
    }
}