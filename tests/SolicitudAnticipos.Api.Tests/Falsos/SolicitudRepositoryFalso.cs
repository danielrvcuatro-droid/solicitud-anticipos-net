using SolicitudAnticipos.Domain.Abstracciones;
using SolicitudAnticipos.Domain.Entities;

namespace SolicitudAnticipos.Api.Tests.Falsos;

/// <summary>
/// Repositorio en memoria para probar <see cref="Api.Aplicacion.IngestaSolicitudService"/> sin
/// depender de EF Core/Supabase (esa parte ya se prueba en SolicitudAnticipos.Infrastructure.Tests).
/// </summary>
internal sealed class SolicitudRepositoryFalso : ISolicitudRepository
{
    private readonly List<Solicitud> _solicitudes = new();

    public int VecesAgregado { get; private set; }

    public Task<Solicitud?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_solicitudes.SingleOrDefault(s => s.Id == id));

    public Task<Solicitud?> ObtenerPorFormsResponseIdAsync(string formsResponseId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_solicitudes.SingleOrDefault(s => s.FormsResponseId == formsResponseId));

    public Task<Solicitud?> ObtenerPorTokenAprobacionAsync(string token, CancellationToken cancellationToken = default) =>
        Task.FromResult(_solicitudes.SingleOrDefault(s => s.Pasos.Any(p => p.TokenAprobacion == token)));

    public Task<IReadOnlyList<Solicitud>> ObtenerConPasosVencidosAsync(DateTimeOffset ahora, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Solicitud>>(Array.Empty<Solicitud>());

    public void Agregar(Solicitud solicitud)
    {
        _solicitudes.Add(solicitud);
        VecesAgregado++;
    }
}
