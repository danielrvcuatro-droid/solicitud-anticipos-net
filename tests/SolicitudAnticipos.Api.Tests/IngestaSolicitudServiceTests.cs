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
        int plazoPorPasoHoras = 48) =>
        new(solicitudes, matriz, unitOfWork, TimeProvider.System,
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
    public async Task Agrega_los_adjuntos_recibidos_del_flujo_de_automate()
    {
        var solicitudes = new SolicitudRepositoryFalso();
        var matriz = new MatrizAprobacionRepositoryFalso(new[]
        {
            MatrizAprobacion.Crear("Desarrollos", 1, 1, "gerente@rvcuatro.com", "Gerente de Área"),
        });
        var servicio = CrearServicio(solicitudes, matriz, new UnitOfWorkFalso());

        var adjuntos = new List<AdjuntoRequest>
        {
            new("factura.pdf", "https://rvcuatro.sharepoint.com/factura.pdf", "application/pdf"),
        };

        var solicitud = await servicio.IngestarAsync(CrearRequest(adjuntos: adjuntos));

        Assert.Single(solicitud.Adjuntos);
        Assert.Equal("factura.pdf", solicitud.Adjuntos.Single().NombreArchivo);
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
