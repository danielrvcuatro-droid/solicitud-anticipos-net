namespace SolicitudAnticipos.Domain.Entities;

/// <summary>
/// Un archivo asociado a la solicitud (adjuntos del formulario, o el PDF de seguimiento generado).
/// El contenido binario vive en SharePoint; aquí solo guardamos la referencia.
/// </summary>
public sealed class Adjunto
{
    public Guid Id { get; private set; }
    public Guid SolicitudId { get; private set; }
    public string NombreArchivo { get; private set; }
    public string UrlSharePoint { get; private set; }
    public string TipoContenido { get; private set; }
    public DateTimeOffset FechaCarga { get; private set; }

    private Adjunto(Guid id, Guid solicitudId, string nombreArchivo, string urlSharePoint, string tipoContenido, DateTimeOffset fechaCarga)
    {
        Id = id;
        SolicitudId = solicitudId;
        NombreArchivo = nombreArchivo;
        UrlSharePoint = urlSharePoint;
        TipoContenido = tipoContenido;
        FechaCarga = fechaCarga;
    }

    internal static Adjunto Crear(Guid solicitudId, string nombreArchivo, string urlSharePoint, string tipoContenido, DateTimeOffset ahora) =>
        new(Guid.NewGuid(), solicitudId, nombreArchivo, urlSharePoint, tipoContenido, ahora);
}
