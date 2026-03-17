namespace Talleres360.Dtos.Responses
{
    public class ApiErrorResponse
    {
        public string Codigo { get; set; }

        public string Mensaje { get; set; }

        public object? Detalles { get; set; }

        public ApiErrorResponse(string codigo, string mensaje, object? detalles = null)
        {
            Codigo = codigo;
            Mensaje = mensaje;
            Detalles = detalles;
        }
    }
}
