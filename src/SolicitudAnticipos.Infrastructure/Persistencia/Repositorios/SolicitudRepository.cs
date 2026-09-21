using Microsoft.EntityFrameworkCore;
using SolicitudAnticipos.Domain.Abstracciones;
using SolicitudAnticipos.Domain.Entities;
using SolicitudAnticipos.Domain.Enums;

namespace SolicitudAnticipos.Infrastructure.Persistencia.Repositorios;

public sealed class SolicitudRepository : ISolicitudRepository
{
    private readonly SolicitudAnticiposDbContext _dbContext;

    public SolicitudRepository(SolicitudAnticiposDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Solicitud?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Solicitudes.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Solicitud?> ObtenerPorFormsResponseIdAsync(string formsResponseId, CancellationToken cancellationToken = default) =>
        _dbContext.Solicitudes.FirstOrDefaultAsync(s => s.FormsResponseId == formsResponseId, cancellationToken);

    public Task<Solicitud?> ObtenerPorTokenAprobacionAsync(string token, CancellationToken cancellationToken = default) =>
        _dbContext.Solicitudes.FirstOrDefaultAsync(s => s.Pasos.Any(p => p.TokenAprobacion == token), cancellationToken);

    public async Task<IReadOnlyList<Solicitud>> ObtenerConPasosVencidosAsync(DateTimeOffset ahora, CancellationToken cancellationToken = default) =>
        await _dbContext.Solicitudes
            .Where(s => s.Estado == EstadoSolicitud.EnAprobacion)
            .Where(s => s.Pasos.Any(p => p.Estado == EstadoPasoAprobacion.Pendiente && p.FechaLimite != null && p.FechaLimite < ahora))
            .ToListAsync(cancellationToken);

    public void Agregar(Solicitud solicitud) => _dbContext.Solicitudes.Add(solicitud);
}
