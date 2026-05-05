# Secretos de Azure Key Vault — Talleres360

Todos los secretos que debes crear en el Key Vault antes del despliegue.
Los nombres usan `--` como separador de jerarquía (convención ASP.NET Core + Key Vault).

## Secretos a crear

| Nombre en Key Vault | Valor |
|---|---|
| `ConnectionStrings--SqlSaas` | Cadena de conexión a la BD SaaS (planes, talleres) |
| `ConnectionStrings--SqlBBDD` | Cadena de conexión a la BD de producción |
| `ConnectionStrings--AzureStorage` | Connection string de Azure Blob Storage |
| `Jwt--Key` | Clave de firma JWT (mínimo 32 caracteres, generala aleatoria) |
| `NotificationsApi--ApiKey` | API key del servicio de notificaciones |

## Configuración del App Service

En **Configuration → Application Settings** del App Service (no en Key Vault, no es un secreto):

| Nombre | Valor |
|---|---|
| `KeyVault__Uri` | `https://<nombre-de-tu-vault>.vault.azure.net/` |

> En App Service los `:` se escriben como `__` en Application Settings.

## Pasos para activar Managed Identity

1. En el App Service → **Identity → System assigned** → activar → guardar
2. En el Key Vault → **Access control (IAM)** → Add role assignment
   - Role: **Key Vault Secrets User**
   - Member: el App Service (por nombre)
3. Desplegar y verificar que la API arranca correctamente

## Flujo de carga de secretos

```
App Service arranca
  → Lee KeyVault:Uri del Application Setting
  → AddAzureKeyVault() con DefaultAzureCredential
  → Managed Identity se autentica automáticamente
  → Secretos inyectados en IConfiguration
  → Código funciona sin cambios
```

## Desarrollo local

Los secretos en local se leen de `appsettings.Development.json` (gitignoreado).
Si quieres usar Key Vault también en local, añade `KeyVault:Uri` a tu `appsettings.Development.json`
y autentícate con `az login` o Visual Studio — `DefaultAzureCredential` lo detecta automáticamente.
