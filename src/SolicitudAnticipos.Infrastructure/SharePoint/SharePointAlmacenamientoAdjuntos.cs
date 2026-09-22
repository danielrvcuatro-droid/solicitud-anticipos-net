using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using SolicitudAnticipos.Domain.Abstracciones;

namespace SolicitudAnticipos.Infrastructure.SharePoint;

/// <summary>
/// Sube los adjuntos (PDFs) a la biblioteca de documentos por defecto de un sitio de SharePoint,
/// usando Microsoft Graph con credenciales de aplicación (client credentials): el backend se
/// autentica como sí mismo, sin que haya un usuario con sesión iniciada de por medio.
/// </summary>
public sealed class SharePointAlmacenamientoAdjuntos : IAlmacenamientoAdjuntos
{
    private static readonly string[] AlcanceGraph = ["https://graph.microsoft.com/.default"];

    private readonly ConfiguracionSharePoint _configuracion;

    // Perezoso a propósito: si se crea en el constructor, CUALQUIER endpoint del controlador que
    // dependa de este servicio (incluso uno que nunca sube adjuntos) fallaría en cuanto el
    // contenedor de DI arme el objeto, si SharePoint todavía no está configurado. Con Lazy<T>,
    // solo se exige la configuración cuando de verdad se intenta subir un archivo.
    private readonly Lazy<GraphServiceClient> _graphClient;

    public SharePointAlmacenamientoAdjuntos(IOptions<ConfiguracionSharePoint> configuracion)
    {
        _configuracion = configuracion.Value;
        _graphClient = new Lazy<GraphServiceClient>(CrearGraphClient);
    }

    public async Task<string> SubirAsync(string carpeta, string nombreArchivo, byte[] contenido, string tipoContenido, CancellationToken cancellationToken = default)
    {
        var graphClient = _graphClient.Value;

        // La biblioteca de documentos por defecto del sitio no se puede direccionar por ruta
        // directamente desde /sites/{id}/drive; hay que resolver primero su Id de "drive".
        var drive = await graphClient.Sites[_configuracion.SiteId].Drive.GetAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"No se pudo resolver la biblioteca de documentos del sitio '{_configuracion.SiteId}'.");

        // Cada solicitud tiene su propia subcarpeta (normalmente su Id): así dos solicitudes que
        // suban un archivo con el mismo nombre ("factura.pdf") no colisionan entre sí, y el
        // archivo conserva su nombre original y legible en SharePoint.
        var rutaDestino = $"{_configuracion.CarpetaDestino}/{carpeta}/{nombreArchivo}";

        // Sintaxis de direccionamiento por ruta de Microsoft Graph: "root:/{ruta}:" en vez del Id del archivo,
        // ya que el archivo todavía no existe (esta llamada lo crea).
        var claveDeRuta = $"root:/{rutaDestino}:";

        using var contenidoStream = new MemoryStream(contenido);

        // Subida simple (PUT directo): cubre archivos de hasta 4 MB, suficiente para la gran
        // mayoría de facturas/comprobantes en PDF. Si en el futuro se necesitan adjuntos más
        // grandes, esto se reemplaza por una sesión de carga (CreateUploadSession).
        var archivoSubido = await graphClient.Drives[drive.Id]
            .Items[claveDeRuta]
            .Content
            .PutAsync(contenidoStream, cancellationToken: cancellationToken);

        return archivoSubido?.WebUrl
            ?? throw new InvalidOperationException($"Microsoft Graph no devolvió la URL del archivo subido ({nombreArchivo}).");
    }

    private GraphServiceClient CrearGraphClient()
    {
        var credencial = new ClientSecretCredential(_configuracion.TenantId, _configuracion.ClientId, _configuracion.ClientSecret);
        return new GraphServiceClient(credencial, AlcanceGraph);
    }
}
