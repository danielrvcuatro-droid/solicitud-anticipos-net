using Microsoft.AspNetCore.Mvc;
using SolicitudAnticipos.Api.Aplicacion;
using SolicitudAnticipos.Api.Contratos;
using SolicitudAnticipos.Domain.Abstracciones;
using SolicitudAnticipos.Domain.Entities;
using SolicitudAnticipos.Domain.Excepciones;

namespace SolicitudAnticipos.Api.Controllers;

[ApiController]
[Route("api/solicitudes")]
public sealed class SolicitudesController : ControllerBase
{
    private readonly IIngestaSolicitudService _ingestaSolicitudService;
    private readonly ISolicitudRepository _solicitudes;

    public SolicitudesController(IIngestaSolicitudService ingestaSolicitudService, ISolicitudRepository solicitudes)
    {
        _ingestaSolicitudService = ingestaSolicitudService;
        _solicitudes = solicitudes;
    }

    /// <summary>
    /// Punto de entrada del flujo mínimo de Power Automate: recibe la respuesta de Forms
    /// (ya con los adjuntos subidos a SharePoint) y crea la solicitud con su cadena de aprobación.
    /// </summary>
    [HttpPost("ingest")]
    [ProducesResponseType(typeof(SolicitudResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Ingestar([FromBody] IngestarSolicitudRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var solicitud = await _ingestaSolicitudService.IngestarAsync(request, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = solicitud.Id }, AMapear(solicitud));
        }
        catch (SolicitudDomainException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SolicitudResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudes.ObtenerPorIdAsync(id, cancellationToken);
        return solicitud is null ? NotFound() : Ok(AMapear(solicitud));
    }

    private static SolicitudResponse AMapear(Solicitud solicitud) =>
        new(solicitud.Id, solicitud.Numero, solicitud.FormsResponseId, solicitud.Estado.ToString(), solicitud.Pasos.Count);
}
