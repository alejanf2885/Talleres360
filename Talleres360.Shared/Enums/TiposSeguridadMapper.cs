using System;

namespace Talleres360.Enums
{
    public static class TiposSeguridadMapper
    {
        public static string ToDbValue(this TipoTokenSeguridad tipo) => tipo switch
        {
            TipoTokenSeguridad.Activacion => "ACTIVACION",
            TipoTokenSeguridad.Invitacion => "INVITACION",
            TipoTokenSeguridad.ResetPassword => "RESET_PASSWORD",
            TipoTokenSeguridad.RefreshToken => "REFRESH_TOKEN",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
        };

        public static string ToDbValue(this TipoVerificacion tipo) => tipo switch
        {
            TipoVerificacion.Email => "EMAIL",
            TipoVerificacion.Telefono => "TELEFONO",
            TipoVerificacion.DosFactores => "DOS_FACTORES",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
        };
    }
}
