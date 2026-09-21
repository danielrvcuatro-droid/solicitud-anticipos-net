using SolicitudAnticipos.Domain.Entities;

namespace SolicitudAnticipos.Domain.Abstracciones;

/// <summary>
/// Puerto hacia la persistencia de <see cref="Solicitud"/>. La implementación (EF Core / Supabase)
/// vive en SolicitudAnticipos.Infrastructure; el dominio y la capa de aplicación solo conocen esta interfaz.
/// </summary>
public interface ISolicitudRepository
{
    Task<Solicitud?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Para no procesar dos veces la misma respuesta de Forms si Automate reintenta la llamada.</summary>
    Task<Solicitud?> ObtenerPorFormsResponseIdAsync(string formsResponseId, CancellationToken cancellationToken = default);

    /// <summary>Resuelve la solicitud dueña de un paso a partir del token de un solo uso del enlace de aprobación.</summary>
    Task<Solicitud?> ObtenerPorTokenAprobacionAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Pasos pendientes cuya fecha límite ya pasó (para que el Worker los marque como vencidos).</summary>
    Task<IReadOnlyList<Solicitud>> ObtenerConPasosVencidosAsync(DateTimeOffset ahora, CancellationToken cancellationToken = default);

    void Agregar(Solicitud solicitud);
}
