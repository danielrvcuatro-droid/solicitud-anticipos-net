namespace SolicitudAnticipos.Infrastructure.SharePoint;

/// <summary>
/// Credenciales y ubicación del sitio de SharePoint donde se guardan los adjuntos, leídas de
/// <c>appsettings</c>/user-secrets (sección <see cref="Seccion"/>). El <see cref="ClientSecret"/>
/// nunca debe commitearse: se configura vía <c>dotnet user-secrets</c> o variables de entorno.
/// </summary>
public sealed class ConfiguracionSharePoint
{
    public const string Seccion = "SharePoint";

    /// <summary>Id del tenant de Microsoft Entra ID (Azure AD) de RVCUATRO.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Id de la aplicación (App Registration) con permiso de Graph <c>Sites.ReadWrite.All</c> (Application).</summary>
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Id del sitio de SharePoint, en el formato que devuelve Graph
    /// (<c>{hostname},{site-collection-id},{site-id}</c>). Se obtiene una única vez con
    /// <c>GET https://graph.microsoft.com/v1.0/sites/{hostname}:/{ruta-del-sitio}</c>.
    /// </summary>
    public string SiteId { get; set; } = string.Empty;

    /// <summary>Carpeta dentro de la biblioteca de documentos por defecto donde se guardan los adjuntos.</summary>
    public string CarpetaDestino { get; set; } = "SolicitudesAnticipos";
}
