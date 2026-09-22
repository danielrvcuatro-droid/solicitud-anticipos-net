namespace SolicitudAnticipos.Domain.Abstracciones;

/// <summary>
/// Puerto hacia el almacenamiento de los archivos adjuntos (PDFs) de una solicitud.
/// La implementación (SharePoint vía Microsoft Graph) vive en Infrastructure; el dominio y la
/// capa de aplicación solo necesitan poder subir un archivo y obtener dónde quedó guardado.
/// </summary>
public interface IAlmacenamientoAdjuntos
{
    /// <summary>
    /// Sube el archivo dentro de una carpeta (típicamente el Id de la solicitud dueña, para que
    /// cada solicitud tenga su propio espacio y los archivos conserven su nombre original sin
    /// colisionar entre sí) y devuelve la URL donde quedó almacenado (para
    /// <see cref="Entities.Adjunto.UrlSharePoint"/>).
    /// </summary>
    Task<string> SubirAsync(string carpeta, string nombreArchivo, byte[] contenido, string tipoContenido, CancellationToken cancellationToken = default);
}
