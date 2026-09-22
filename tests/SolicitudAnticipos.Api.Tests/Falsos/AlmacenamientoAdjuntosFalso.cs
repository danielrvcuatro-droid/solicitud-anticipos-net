using SolicitudAnticipos.Domain.Abstracciones;

namespace SolicitudAnticipos.Api.Tests.Falsos;

/// <summary>
/// Reemplaza la subida real a SharePoint/Microsoft Graph en las pruebas: en vez de contactar un
/// tenant real, simplemente "sube" el archivo en memoria y devuelve una URL falsa predecible.
/// </summary>
internal sealed class AlmacenamientoAdjuntosFalso : IAlmacenamientoAdjuntos
{
    public List<(string Carpeta, string NombreArchivo, byte[] Contenido, string TipoContenido)> ArchivosSubidos { get; } = new();

    public Task<string> SubirAsync(string carpeta, string nombreArchivo, byte[] contenido, string tipoContenido, CancellationToken cancellationToken = default)
    {
        ArchivosSubidos.Add((carpeta, nombreArchivo, contenido, tipoContenido));
        return Task.FromResult($"https://rvcuatro.sharepoint.com/sites/falso/{carpeta}/{nombreArchivo}");
    }
}
