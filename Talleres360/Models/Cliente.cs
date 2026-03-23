using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("Clientes")]  
    public class Cliente
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TallerId")]
        public int? TallerId { get; set; }  

        [Column("Nombre")]
        [Required, StringLength(100)]  
        public string Nombre { get; set; } = string.Empty;

        [Column("Apellidos")]
        [StringLength(150)]  
        public string? Apellidos { get; set; }

        [Column("NifCif")]
        [StringLength(20)]
        public string? NifCif { get; set; }

        [Column("EsEmpresa")]
        public bool EsEmpresa { get; set; } = false;

        [Column("Direccion")]
        [StringLength(500)]
        public string? Direccion { get; set; }

        [Column("CodigoPostal")]
        [StringLength(15)]
        public string? CodigoPostal { get; set; }

        [Column("Localidad")]
        [StringLength(150)]
        public string? Localidad { get; set; }

        [Column("Provincia")]
        [StringLength(150)]
        public string? Provincia { get; set; }

        [Column("Telefono")]
        [Required, StringLength(20)] 
        public string Telefono { get; set; } = string.Empty;

        [Column("Email")]
        [StringLength(150)] 
        public string? Email { get; set; }

        [Column("AceptaComunicaciones")]
        public bool AceptaComunicaciones { get; set; } = false;

        [Column("FechaFirmaRgpd")]  
        public DateTime? FechaFirmaRgpd { get; set; }

        [Column("FirmaDigitalUrl")]
        [StringLength(500)]  
        public string? FirmaDigitalUrl { get; set; }

        [Column("Eliminado")]
        public bool Eliminado { get; set; } = false;

        [Column("FechaCreacion")]
        [Required]
        public DateTime FechaCreacion { get; set; }

        [Column("FechaModificacion")]
        public DateTime? FechaModificacion { get; set; }
    }
}