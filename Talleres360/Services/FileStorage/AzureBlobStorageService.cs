using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Talleres360.Enums;
using Talleres360.Interfaces.FileStorage;

namespace Talleres360.Services.FileStorage
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName = "talleres360-imagenes";

        public AzureBlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureStorage")
                               ?? throw new ArgumentNullException("AzureStorage connection string is missing");
        }

        public async Task BorrarArchivoAsync(string rutaRelativa)
        {
            if (string.IsNullOrEmpty(rutaRelativa)) return;

            if (rutaRelativa.Contains("default-avatar.png", StringComparison.OrdinalIgnoreCase)) return;

            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            // Extraer ruta del blob desde URL absoluta (ej: .../talleres360-imagenes/usuarios/foto.webp → usuarios/foto.webp)
            string blobPath = rutaRelativa;
            if (Uri.TryCreate(rutaRelativa, UriKind.Absolute, out Uri? uri))
            {
                string containerPrefix = $"/{_containerName}/";
                int idx = uri.AbsolutePath.IndexOf(containerPrefix, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                    blobPath = Uri.UnescapeDataString(uri.AbsolutePath[(idx + containerPrefix.Length)..]);
            }

            BlobClient blobClient = containerClient.GetBlobClient(blobPath);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<string> GuardarArchivoAsync(Stream contenido, string nombreArchivo, CarpetaDestino carpeta)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            try
            {
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            string nombreCarpeta = carpeta.ToString().ToLower();
            string rutaBlob = $"{nombreCarpeta}/{nombreArchivo}";

            BlobClient blobClient = containerClient.GetBlobClient(rutaBlob);

            BlobUploadOptions uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = "image/webp" }
            };

            await blobClient.UploadAsync(contenido, uploadOptions);

            return blobClient.Uri.ToString();
        }
    }
}
