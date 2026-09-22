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
    private readonly IAlmacenamientoAdjuntos _almacenamientoAdjuntos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ConfiguracionAprobacion _configuracion;

    public IngestaSolicitudService(
        ISolicitudRepository solicitudes,
        IMatrizAprobacionRepository matrizAprobacion,
        IAlmacenamientoAdjuntos almacenamientoAdjuntos,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IOptions<ConfiguracionAprobacion> configuracion)
    {
        _solicitudes = solicitudes;
        _matrizAprobacion = matrizAprobacion;
        _almacenamientoAdjuntos = almacenamientoAdjuntos;
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

        // Cada solicitud sube sus adjuntos a su propia subcarpeta (su Id, ya asignado por
        // Solicitud.Crear) para que el nombre original del archivo no colisione con el de otra
        // solicitud ni se tenga que ensuciar con un prefijo aleatorio.
        var carpetaAdjuntos = solicitud.Id.ToString();

        foreach (var adjunto in request.Adjuntos ?? [])
        {
            var tipoContenido = adjunto.TipoContenido ?? "application/octet-stream";
            var contenido = Convert.FromBase64String(adjunto.ContenidoBase64);
            var urlSharePoint = await _almacenamientoAdjuntos.SubirAsync(carpetaAdjuntos, adjunto.NombreArchivo, contenido, tipoContenido, cancellationToken);

            solicitud.AgregarAdjunto(adjunto.NombreArchivo, urlSharePoint, tipoContenido, ahora);
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
