using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("CLIENTES")]
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TallerId { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Apellidos { get; set; }

        // --- NUEVOS CAMPOS FISCALES (Opcionales para Tickets, Obligatorios para Facturas Pro) ---

        [StringLength(20)]
        public string? NifCif { get; set; } // DNI, NIE o CIF

        public bool EsEmpresa { get; set; } = false;

        [StringLength(500)]
        public string? Direccion { get; set; }

        [StringLength(15)]
        public string? CodigoPostal { get; set; }

        [StringLength(150)]
        public string? Localidad { get; set; }

        [StringLength(150)]
        public string? Provincia { get; set; }

        // ---------------------------------------------------------------------------------------

        [Required]
        [StringLength(40)]
        public string Telefono { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Email { get; set; }

        public bool AceptaComunicaciones { get; set; }

        public DateTime? FechaFirmaRGPD { get; set; }

        [StringLength(1000)]
        public string? FirmaDigitalUrl { get; set; }

        public bool Eliminado { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaModificacion { get; set; }

    }
}