namespace Talleres360.Dtos.Seguridad
{
    public class AccesoResult
    {
        public bool PuedeAcceder { get; set; }
        public string? Mensaje { get; set; }
        public string? ErrorCode { get; set; }

        public static AccesoResult Permitido()
        {
            return new AccesoResult { PuedeAcceder = true };
        }

        public static AccesoResult Denegado(string mensaje, string? errorCode = null)
        {
            return new AccesoResult
            {
                PuedeAcceder = false,
                Mensaje = mensaje,
                ErrorCode = errorCode
            };
        }
    }
}