using SolicitudAnticipos.Api.Contratos;
using SolicitudAnticipos.Domain.Entities;

namespace SolicitudAnticipos.Api.Aplicacion;

public interface IIngestaSolicitudService
{
    /// <summary>
    /// Crea la <see cref="Solicitud"/> a partir del payload de Automate, arma su cadena de
    /// aprobación según la matriz configurable del departamento, y la persiste.
    /// Es idempotente por <see cref="IngestarSolicitudRequest.FormsResponseId"/>: si Automate
    /// reintenta la misma respuesta de Forms, devuelve la solicitud ya creada sin duplicarla.
    /// </summary>
    Task<Solicitud> IngestarAsync(IngestarSolicitudRequest request, CancellationToken cancellationToken = default);
}
