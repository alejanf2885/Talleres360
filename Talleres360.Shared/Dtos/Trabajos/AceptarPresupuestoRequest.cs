namespace Talleres360.Dtos.Trabajos
{
    /// <summary>Request body para aceptar un presupuesto.</summary>
    public class AceptarPresupuestoRequest
    {
        /// <summary>URL de la imagen con la firma de aceptación del cliente (opcional).</summary>
        public string? FirmaAceptacionUrl { get; set; }
    }
}
