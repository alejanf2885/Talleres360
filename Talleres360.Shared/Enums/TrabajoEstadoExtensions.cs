namespace Talleres360.Enums
{
    /// <summary>
    /// Máquina de estados y helpers para TrabajoEstado.
    /// Centraliza las reglas de transición y clasificación de estados.
    /// </summary>
    public static class TrabajoEstadoExtensions
    {
        // =====================================================================
        //  CLASIFICACIÓN DE ESTADOS
        // =====================================================================

        /// <summary>Estados que representan un presupuesto (no aparecen en listados de trabajos).</summary>
        public static readonly TrabajoEstado[] EstadosPresupuesto =
        {
            TrabajoEstado.PRESUPUESTO,
            TrabajoEstado.PRESUPUESTO_ENVIADO,
            TrabajoEstado.RECHAZADO,
            TrabajoEstado.CADUCADO
        };

        /// <summary>Estados que representan un trabajo activo (no aparecen en listados de presupuestos).</summary>
        public static readonly TrabajoEstado[] EstadosTrabajo =
        {
            TrabajoEstado.ABIERTO,
            TrabajoEstado.EN_PROCESO,
            TrabajoEstado.PENDIENTE_PIEZAS,
            TrabajoEstado.CERRADO,
            TrabajoEstado.FACTURADO
        };

        /// <summary>Estados terminales (no permiten transición a ningún otro estado).</summary>
        public static readonly TrabajoEstado[] EstadosTerminales =
        {
            TrabajoEstado.FACTURADO,
            TrabajoEstado.RECHAZADO,
            TrabajoEstado.CADUCADO,
            TrabajoEstado.CANCELADO
        };

        // =====================================================================
        //  HELPERS DE INSTANCIA (extension methods)
        // =====================================================================

        /// <summary>¿Este estado pertenece al ciclo de presupuesto?</summary>
        public static bool EsPresupuesto(this TrabajoEstado estado) =>
            estado is TrabajoEstado.PRESUPUESTO
                  or TrabajoEstado.PRESUPUESTO_ENVIADO
                  or TrabajoEstado.RECHAZADO
                  or TrabajoEstado.CADUCADO;

        /// <summary>¿Este estado pertenece al ciclo de trabajo activo?</summary>
        public static bool EsTrabajo(this TrabajoEstado estado) =>
            estado is TrabajoEstado.ABIERTO
                  or TrabajoEstado.EN_PROCESO
                  or TrabajoEstado.PENDIENTE_PIEZAS
                  or TrabajoEstado.CERRADO
                  or TrabajoEstado.FACTURADO;

        /// <summary>¿Es un estado terminal (sin transiciones salientes)?</summary>
        public static bool EsTerminal(this TrabajoEstado estado) =>
            estado is TrabajoEstado.FACTURADO
                  or TrabajoEstado.RECHAZADO
                  or TrabajoEstado.CADUCADO
                  or TrabajoEstado.CANCELADO;

        /// <summary>¿Se puede editar el documento en este estado?</summary>
        public static bool PermiteEdicion(this TrabajoEstado estado) =>
            estado is TrabajoEstado.PRESUPUESTO
                  or TrabajoEstado.PRESUPUESTO_ENVIADO;

        // =====================================================================
        //  MÁQUINA DE ESTADOS — Transiciones válidas
        // =====================================================================

        /// <summary>
        /// Obtiene las transiciones permitidas desde un estado dado.
        /// </summary>
        public static TrabajoEstado[] ObtenerTransicionesPermitidas(this TrabajoEstado estado) => estado switch
        {
            TrabajoEstado.PRESUPUESTO          => new[] { TrabajoEstado.PRESUPUESTO_ENVIADO, TrabajoEstado.ABIERTO, TrabajoEstado.CANCELADO },
            TrabajoEstado.PRESUPUESTO_ENVIADO   => new[] { TrabajoEstado.ABIERTO, TrabajoEstado.RECHAZADO, TrabajoEstado.CADUCADO, TrabajoEstado.CANCELADO },
            TrabajoEstado.ABIERTO               => new[] { TrabajoEstado.EN_PROCESO, TrabajoEstado.PENDIENTE_PIEZAS, TrabajoEstado.CANCELADO },
            TrabajoEstado.EN_PROCESO            => new[] { TrabajoEstado.PENDIENTE_PIEZAS, TrabajoEstado.CERRADO, TrabajoEstado.CANCELADO },
            TrabajoEstado.PENDIENTE_PIEZAS      => new[] { TrabajoEstado.EN_PROCESO, TrabajoEstado.CANCELADO },
            TrabajoEstado.CERRADO               => new[] { TrabajoEstado.FACTURADO, TrabajoEstado.CANCELADO },
            TrabajoEstado.FACTURADO             => Array.Empty<TrabajoEstado>(),
            TrabajoEstado.RECHAZADO             => Array.Empty<TrabajoEstado>(),
            TrabajoEstado.CADUCADO              => Array.Empty<TrabajoEstado>(),
            TrabajoEstado.CANCELADO             => Array.Empty<TrabajoEstado>(),
            _ => Array.Empty<TrabajoEstado>()
        };

        /// <summary>
        /// Valida si una transición de estado es permitida.
        /// </summary>
        public static bool PuedeTransicionarA(this TrabajoEstado estadoActual, TrabajoEstado estadoNuevo) =>
            estadoActual.ObtenerTransicionesPermitidas().Contains(estadoNuevo);
    }
}
