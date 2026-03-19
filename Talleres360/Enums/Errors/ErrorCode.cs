namespace Talleres360.Enums.Errors
{
    public enum ErrorCode
    {
        // --- AUTH: Sesión, Acceso y Seguridad ---
        AUTH_CREDENCIALES_INCORRECTAS,
        AUTH_CUENTA_INACTIVA,
        AUTH_CUENTA_BLOQUEADA,
        AUTH_CUENTA_YA_ACTIVA,
        AUTH_TOKEN_INVALIDO,
        AUTH_TOKEN_EXPIRADO,
        AUTH_EMAIL_NO_VERIFICADO,
        AUTH_REFRESH_TOKEN_INVALIDO,
        AUTH_REFRESH_TOKEN_EXPIRADO,
        AUTH_LOGOUT_FALLIDO,
        AUTH_NO_AUTORIZADO,
        AUTH_FORBIDDEN,
        AUTH_ACCESO_DENEGADO,         // Usado en TallerAuthorize
        AUTH_REVOCACION_FALLIDA,

        // --- REG: Registro de Taller y Onboarding ---
        REG_FALLIDO,
        REG_PLAN_NO_ENCONTRADO,
        REG_EMAIL_YA_REGISTRADO,
        REG_CIF_DUPLICADO,
        REG_ERROR_SUBIDA_IMAGEN,
        REG_ERROR_CREACION_USUARIO,
        REG_TALLER_YA_EXISTE,

        // --- SUBS: Suscripciones y Pagos (SaaS) ---
        SUBS_SIN_PLAN_ACTIVO,
        SUBS_LIMITE_ALCANZADO,
        SUBS_PAGO_RECHAZADO,

        // --- CUST: Gestión de Clientes ---
        CUST_NO_ENCONTRADO,
        CUST_DNI_DUPLICADO,
        CUST_EMAIL_DUPLICADO,
        CUST_TELEFONO_INVALIDO,
        CUST_SIN_FIRMA_RGPD,
        CUST_ERROR_ELIMINACION,
        CUST_LIMITE_PLAN_ALCANZADO,

        // --- VEH: Gestión de Vehículos  ---
        VEH_NO_ENCONTRADO,
        VEH_MATRICULA_DUPLICADA,
        VEH_VIN_INVALIDO,
        VEH_ERROR_MAQUINA_ESTADO,    
        VEH_MARCA_NO_ENCONTRADA,
        VEH_MODELO_NO_ENCONTRADA,

        // --- SYS: Sistema y Errores Globales ---
        SYS_DATOS_INVALIDOS,
        SYS_ERROR_GENERICO,
        SYS_ENTIDAD_NO_ENCONTRADA,
        SYS_ARCHIVO_DEMASIADO_GRANDE,
        SYS_OPERACION_INVALIDA,
        SYS_ERROR_BASE_DATOS,
        SYS_SERVICIO_NO_DISPONIBLE
    }
}