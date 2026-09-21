using Microsoft.Extensions.Options;
using SolicitudAnticipos.Api.Configuracion;
using SolicitudAnticipos.Api.Contratos;
using SolicitudAnticipos.Domain.Abstracciones;
using SolicitudAnticipos.Domain.Entities;
using SolicitudAnticipos.Domain.Politicas;

namespace SolicitudAnticipos.Api.Aplicacion;

public sealed class IngestaSolicitudService : IIngestaSolicitudService
{
    private readonly ISolicitudRepository _solicitudes;
    private readonly IMatrizAprobacionRepository _matrizAprobacion;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ConfiguracionAprobacion _configuracion;

    public IngestaSolicitudService(
        ISolicitudRepository solicitudes,
        IMatrizAprobacionRepository matrizAprobacion,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IOptions<ConfiguracionAprobacion> configuracion)
    {
        _solicitudes = solicitudes;
        _matrizAprobacion = matrizAprobacion;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _configuracion = configuracion.Value;
    }

    public async Task<Solicitud> IngestarAsync(IngestarSolicitudRequest request, CancellationToken cancellationToken = default)
    {
        var existente = await _solicitudes.ObtenerPorFormsResponseIdAsync(request.FormsResponseId, cancellationToken);
        if (existente is not null)
        {
            // Automate reintentó el envío de la misma respuesta de Forms: no duplicar la solicitud.
            return existente;
        }

        var ahora = _timeProvider.GetUtcNow();

        var solicitud = Solicitud.Crear(
            request.FormsResponseId,
            request.SolicitanteEmail,
            request.SolicitanteNombre,
            request.Sociedad,
            request.Departamento,
            request.Monto,
            request.EsUrgente,
            request.IncluyeAnalistaFinanciero,
            request.ModoAprobacion,
            ahora);

        foreach (var adjunto in request.Adjuntos ?? [])
        {
            solicitud.AgregarAdjunto(adjunto.NombreArchivo, adjunto.UrlSharePoint, adjunto.TipoContenido ?? "application/octet-stream", ahora);
        }

        var filasMatriz = await _matrizAprobacion.ObtenerPorDepartamentoAsync(request.Departamento, cancellationToken);
        var plan = ConstructorPlanAprobacion.Construir(filasMatriz, request.IncluyeAnalistaFinanciero);
        var plazoPorPaso = TimeSpan.FromHours(_configuracion.PlazoPorPasoHoras);

        solicitud.IniciarAprobacion(plan, plazoPorPaso, ahora);

        _solicitudes.Agregar(solicitud);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return solicitud;
    }
}
