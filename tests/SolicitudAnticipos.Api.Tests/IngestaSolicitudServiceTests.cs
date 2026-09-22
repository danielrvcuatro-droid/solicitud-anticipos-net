using Microsoft.Extensions.Options;
using SolicitudAnticipos.Api.Aplicacion;
using SolicitudAnticipos.Api.Configuracion;
using SolicitudAnticipos.Api.Contratos;
using SolicitudAnticipos.Api.Tests.Falsos;
using SolicitudAnticipos.Domain.Entities;
using SolicitudAnticipos.Domain.Enums;
using SolicitudAnticipos.Domain.Excepciones;

namespace SolicitudAnticipos.Api.Tests;

public sealed class IngestaSolicitudServiceTests
{
    private static IngestaSolicitudService CrearServicio(
        SolicitudRepositoryFalso solicitudes,
        MatrizAprobacionRepositoryFalso matriz,
        UnitOfWorkFalso unitOfWork,
        AlmacenamientoAdjuntosFalso? almacenamientoAdjuntos = null,
        int plazoPorPasoHoras = 48) =>
        new(solicitudes, matriz, almacenamientoAdjuntos ?? new AlmacenamientoAdjuntosFalso(), unitOfWork, TimeProvider.System,
            Options.Create(new ConfiguracionAprobacion { PlazoPorPasoHoras = plazoPorPasoHoras }));

    private static IngestarSolicitudRequest CrearRequest(
        string formsResponseId = "resp-1",
        string departamento = "Desarrollos",
        bool incluyeAnalistaFinanciero = false,
        IReadOnlyList<AdjuntoRequest>? adjuntos = null) =>
        new(
            FormsResponseId: formsResponseId,
            SolicitanteEmail: "colaborador@rvcuatro.com",
            SolicitanteNombre: "Colaborador de Prueba",
            Sociedad: "RVCUATRO",
            Departamento: departamento,
            Monto: 2500m,
            EsUrgente: false,
            IncluyeAnalistaFinanciero: incluyeAnalistaFinanciero,
            ModoAprobacion: ModoAprobacion.Secuencial,
            Adjuntos: adjuntos);

    [Fact]
    public async Task Ingresa_una_solicitud_nueva_y_arma_la_cadena_de_aprobacion_desde_la_matriz()
    {
        var solicitudes = new SolicitudRepositoryFalso();
        var matriz = new MatrizAprobacionRepositoryFalso(new[]
        {
            MatrizAprobacion.Crear("Desarrollos", nivel: 1, orden: 1, "gerente@rvcuatro.com", "Gerente de Área"),
            MatrizAprobacion.Crear("Desarrollos", nivel: 2, orden: 1, "analista@rvcuatro.com", "Analista Financiero", esAnalistaFinanciero: true),
        });
        var unitOfWork = new UnitOfWorkFalso();
        var servicio = CrearServicio(solicitudes, matriz, unitOfWork);

        var solicitud = await servicio.IngestarAsync(CrearRequest(incluyeAnalistaFinanciero: false));

        Assert.Equal(EstadoSolicitud.EnAprobacion, solicitud.Estado);
        Assert.Single(solicitud.Pasos); // se excluyó la fila del analista financiero
        Assert.Equal("gerente@rvcuatro.com", solicitud.Pasos.Single().AprobadorEmail);
        Assert.Equal(1, unitOfWork.VecesGuardado);
        Assert.Equal(1, solicitudes.VecesAgregado);
    }

    [Fact]
    public async Task Incluye_al_analista_financiero_cuando_la_solicitud_lo_requiere()
    {
        var solicitudes = new SolicitudRepositoryFalso();
        var matriz = new MatrizAprobacionRepositoryFalso(new[]
        {
            MatrizAprobacion.Crear("Desarrollos", nivel: 1, orden: 1, "gerente@rvcuatro.com", "Gerente de Área"),
            MatrizAprobacion.Crear("Desarrollos", nivel: 2, orden: 1, "analista@rvcuatro.com", "Analista Financiero", esAnalistaFinanciero: true),
        });
        var servicio = CrearServicio(solicitudes, matriz, new UnitOfWorkFalso());

        var solicitud = await servicio.IngestarAsync(CrearRequest(incluyeAnalistaFinanciero: true));

        Assert.Equal(2, solicitud.Pasos.Count);
    }

    [Fact]
    public async Task Sube_los_adjuntos_a_SharePoint_y_los_agrega_con_la_url_resultante()
    {
        var solicitudes = new SolicitudRepositoryFalso();
        var matriz = new MatrizAprobacionRepositoryFalso(new[]
        {
            MatrizAprobacion.Crear("Desarrollos", 1, 1, "gerente@rvcuatro.com", "Gerente de Área"),
        });
        var almacenamiento = new AlmacenamientoAdjuntosFalso();
        var servicio = CrearServicio(solicitudes, matriz, new UnitOfWorkFalso(), almacenamiento);

        var contenidoOriginal = "contenido de prueba"u8.ToArray();
        var adjuntos = new List<AdjuntoRequest>
        {
            new("factura.pdf", Convert.ToBase64String(contenidoOriginal), "application/pdf"),
        };

        var solicitud = await servicio.IngestarAsync(CrearRequest(adjuntos: adjuntos));

        // El servicio debió llamar al almacenamiento con el contenido ya decodificado, subiéndolo
        // a la carpeta de esta solicitud (su Id) para que el nombre no colisione con el de otra...
        Assert.Single(almacenamiento.ArchivosSubidos);
        var archivoSubido = almacenamiento.ArchivosSubidos.Single();
        Assert.Equal(contenidoOriginal, archivoSubido.Contenido);
        Assert.Equal(solicitud.Id.ToString(), archivoSubido.Carpeta);

        // ...y guardar el adjunto de la solicitud con la URL que ese almacenamiento devolvió (no con el base64).
        Assert.Single(solicitud.Adjuntos);
        var adjunto = solicitud.Adjuntos.Single();
        Assert.Equal("factura.pdf", adjunto.NombreArchivo);
        Assert.Equal($"https://rvcuatro.sharepoint.com/sites/falso/{solicitud.Id}/factura.pdf", adjunto.UrlSharePoint);
    }

    [Fact]
    public async Task Es_idempotente_ante_reintentos_del_mismo_FormsResponseId()
    {
        var solicitudes = new SolicitudRepositoryFalso();
        var matriz = new MatrizAprobacionRepositoryFalso(new[]
        {
            MatrizAprobacion.Crear("Desarrollos", 1, 1, "gerente@rvcuatro.com", "Gerente de Área"),
        });
        var unitOfWork = new UnitOfWorkFalso();
        var servicio = CrearServicio(solicitudes, matriz, unitOfWork);

        var primera = await servicio.IngestarAsync(CrearRequest(formsResponseId: "resp-duplicado"));
        var segunda = await servicio.IngestarAsync(CrearRequest(formsResponseId: "resp-duplicado"));

        Assert.Equal(primera.Id, segunda.Id);
        Assert.Equal(1, solicitudes.VecesAgregado);
        Assert.Equal(1, unitOfWork.VecesGuardado);
    }

    [Fact]
    public async Task Lanza_excepcion_de_dominio_si_el_departamento_no_tiene_aprobadores_activos_en_la_matriz()
    {
        var solicitudes = new SolicitudRepositoryFalso();
        var matriz = new MatrizAprobacionRepositoryFalso(); // sin filas configuradas
        var servicio = CrearServicio(solicitudes, matriz, new UnitOfWorkFalso());

        await Assert.ThrowsAsync<SolicitudDomainException>(() => servicio.IngestarAsync(CrearRequest(departamento: "SinMatriz")));
    }
}
