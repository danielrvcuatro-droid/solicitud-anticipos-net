using SolicitudAnticipos.Domain.Enums;
using SolicitudAnticipos.Domain.Excepciones;
using SolicitudAnticipos.Domain.Politicas;
using Xunit;

namespace SolicitudAnticipos.Domain.Tests;

public class SolicitudTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 16, 8, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Plazo = TimeSpan.FromHours(48);

    private static Entities.Solicitud CrearSolicitudDePrueba(bool esUrgente = false, bool incluyeAnalistaFinanciero = false, ModoAprobacion modo = ModoAprobacion.Secuencial) =>
        Entities.Solicitud.Crear(
            numero: 1,
            formsResponseId: "resp-123",
            solicitanteEmail: "colaborador@rvcuatro.com",
            solicitanteNombre: "Colaborador de Prueba",
            sociedad: "RVCUATRO",
            departamento: "Desarrollos",
            monto: 1500m,
            esUrgente: esUrgente,
            incluyeAnalistaFinanciero: incluyeAnalistaFinanciero,
            modoAprobacion: modo,
            ahora: Ahora);

    [Fact]
    public void Crear_deja_la_solicitud_en_estado_Pendiente_con_evento_en_bitacora()
    {
        var solicitud = CrearSolicitudDePrueba();

        Assert.Equal(EstadoSolicitud.Pendiente, solicitud.Estado);
        Assert.Single(solicitud.Bitacora);
        Assert.Equal(TipoEventoBitacora.SolicitudCreada, solicitud.Bitacora.Single().Tipo);
    }

    [Fact]
    public void Crear_rechaza_monto_invalido()
    {
        var excepcion = Assert.Throws<SolicitudDomainException>(() =>
            Entities.Solicitud.Crear(1, "resp-1", "a@rvcuatro.com", "A", "RVCUATRO", "Desarrollos", 0m, false, false, ModoAprobacion.Secuencial, Ahora));

        Assert.Contains("mayor a cero", excepcion.Message);
    }

    [Fact]
    public void IniciarAprobacion_en_modo_secuencial_solo_notifica_al_primer_aprobador()
    {
        var solicitud = CrearSolicitudDePrueba();
        var plan = new List<PlanAprobador>
        {
            new(1, Nivel: 1, "gerente@rvcuatro.com", "Gerente de Área"),
            new(2, Nivel: 1, "director@rvcuatro.com", "Director"),
        };

        solicitud.IniciarAprobacion(plan, Plazo, Ahora);

        Assert.Equal(EstadoSolicitud.EnAprobacion, solicitud.Estado);
        var pendientes = solicitud.ObtenerPasosPendientesDeNotificar();
        Assert.Single(pendientes);
        Assert.Equal("gerente@rvcuatro.com", pendientes[0].AprobadorEmail);
    }

    [Fact]
    public void IniciarAprobacion_en_modo_paralelo_notifica_a_todo_el_nivel()
    {
        var solicitud = CrearSolicitudDePrueba(modo: ModoAprobacion.Paralelo);
        var plan = new List<PlanAprobador>
        {
            new(1, Nivel: 1, "gerente@rvcuatro.com", "Gerente de Área"),
            new(2, Nivel: 1, "analista@rvcuatro.com", "Analista Financiero"),
        };

        solicitud.IniciarAprobacion(plan, Plazo, Ahora);

        Assert.Equal(2, solicitud.ObtenerPasosPendientesDeNotificar().Count);
    }

    [Fact]
    public void Aprobar_todos_los_pasos_deja_la_solicitud_Aprobada()
    {
        var solicitud = CrearSolicitudDePrueba();
        var plan = new List<PlanAprobador>
        {
            new(1, 1, "gerente@rvcuatro.com", "Gerente"),
            new(2, 2, "director@rvcuatro.com", "Director"),
        };
        solicitud.IniciarAprobacion(plan, Plazo, Ahora);

        var tokenGerente = solicitud.Pasos.Single(p => p.AprobadorEmail == "gerente@rvcuatro.com").TokenAprobacion;
        solicitud.RegistrarRespuesta(tokenGerente, aprobado: true, comentario: null, Plazo, Ahora.AddHours(1));

        var tokenDirector = solicitud.Pasos.Single(p => p.AprobadorEmail == "director@rvcuatro.com").TokenAprobacion;
        solicitud.RegistrarRespuesta(tokenDirector, aprobado: true, comentario: "OK", Plazo, Ahora.AddHours(2));

        Assert.Equal(EstadoSolicitud.Aprobada, solicitud.Estado);
        Assert.All(solicitud.Pasos, p => Assert.Equal(EstadoPasoAprobacion.Aprobado, p.Estado));
    }

    [Fact]
    public void Rechazar_un_paso_detiene_el_flujo_y_omite_los_pasos_restantes()
    {
        var solicitud = CrearSolicitudDePrueba();
        var plan = new List<PlanAprobador>
        {
            new(1, 1, "gerente@rvcuatro.com", "Gerente"),
            new(2, 2, "director@rvcuatro.com", "Director"),
        };
        solicitud.IniciarAprobacion(plan, Plazo, Ahora);

        var tokenGerente = solicitud.Pasos.Single(p => p.AprobadorEmail == "gerente@rvcuatro.com").TokenAprobacion;
        solicitud.RegistrarRespuesta(tokenGerente, aprobado: false, comentario: "No cumple política", Plazo, Ahora.AddHours(1));

        Assert.Equal(EstadoSolicitud.Rechazada, solicitud.Estado);
        Assert.Equal("No cumple política", solicitud.ComentarioResolucion);
        Assert.Equal("Gerente", solicitud.NombreResolutor);

        var pasoDirector = solicitud.Pasos.Single(p => p.AprobadorEmail == "director@rvcuatro.com");
        Assert.Equal(EstadoPasoAprobacion.Omitido, pasoDirector.Estado);
    }

    [Fact]
    public void MarcarPasoVencido_deja_la_solicitud_Vencida_y_omite_el_resto()
    {
        var solicitud = CrearSolicitudDePrueba();
        var plan = new List<PlanAprobador>
        {
            new(1, 1, "gerente@rvcuatro.com", "Gerente"),
            new(2, 2, "director@rvcuatro.com", "Director"),
        };
        solicitud.IniciarAprobacion(plan, Plazo, Ahora);

        var pasoGerente = solicitud.Pasos.Single(p => p.AprobadorEmail == "gerente@rvcuatro.com");
        solicitud.MarcarPasoVencido(pasoGerente.Id, Ahora.AddHours(49));

        Assert.Equal(EstadoSolicitud.Vencida, solicitud.Estado);
        Assert.Equal(EstadoPasoAprobacion.Vencido, pasoGerente.Estado);
        Assert.All(solicitud.Pasos.Where(p => p.Id != pasoGerente.Id), p => Assert.Equal(EstadoPasoAprobacion.Omitido, p.Estado));
    }

    [Fact]
    public void No_se_puede_responder_dos_veces_el_mismo_paso()
    {
        var solicitud = CrearSolicitudDePrueba();
        solicitud.IniciarAprobacion(new List<PlanAprobador> { new(1, 1, "gerente@rvcuatro.com", "Gerente") }, Plazo, Ahora);

        var token = solicitud.Pasos.Single().TokenAprobacion;
        solicitud.RegistrarRespuesta(token, true, null, Plazo, Ahora.AddHours(1));

        Assert.Throws<SolicitudDomainException>(() =>
            solicitud.RegistrarRespuesta(token, true, null, Plazo, Ahora.AddHours(2)));
    }

    [Fact]
    public void No_se_puede_iniciar_aprobacion_con_plan_vacio()
    {
        var solicitud = CrearSolicitudDePrueba();

        Assert.Throws<SolicitudDomainException>(() =>
            solicitud.IniciarAprobacion(new List<PlanAprobador>(), Plazo, Ahora));
    }
}
