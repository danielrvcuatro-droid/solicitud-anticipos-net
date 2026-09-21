using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SolicitudAnticipos.Domain.Enums;
using SolicitudAnticipos.Domain.Politicas;
using SolicitudAnticipos.Infrastructure.Persistencia;
using Xunit;

namespace SolicitudAnticipos.Infrastructure.Tests;

/// <summary>
/// Prueba de integración liviana: usa SQLite en memoria (no Supabase real) solo para verificar
/// que el mapeo de EF Core -incluyendo las colecciones privadas del agregado Solicitud
/// (Pasos/Adjuntos/Bitacora) respaldadas por campos- guarda y recupera datos correctamente.
/// </summary>
public sealed class SolicitudPersistenciaTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<SolicitudAnticiposDbContext> _opciones;

    public SolicitudPersistenciaTests()
    {
        _conexion = new SqliteConnection("Filename=:memory:");
        _conexion.Open();

        _opciones = new DbContextOptionsBuilder<SolicitudAnticiposDbContext>()
            .UseSqlite(_conexion)
            .Options;

        using var contexto = new SolicitudAnticiposDbContext(_opciones);
        contexto.Database.EnsureCreated();
    }

    public void Dispose() => _conexion.Dispose();

    [Fact]
    public async Task Guarda_y_recupera_una_solicitud_con_su_cadena_de_aprobacion_completa()
    {
        var ahora = DateTimeOffset.UtcNow;
        var solicitud = Domain.Entities.Solicitud.Crear(
            formsResponseId: "resp-abc-123",
            solicitanteEmail: "colaborador@rvcuatro.com",
            solicitanteNombre: "Colaborador de Prueba",
            sociedad: "RVCUATRO",
            departamento: "Desarrollos",
            monto: 2500m,
            esUrgente: true,
            incluyeAnalistaFinanciero: true,
            modoAprobacion: ModoAprobacion.Secuencial,
            ahora: ahora);

        var plan = new List<PlanAprobador>
        {
            new(1, 1, "gerente@rvcuatro.com", "Gerente de Área"),
            new(2, 2, "analista@rvcuatro.com", "Analista Financiero"),
        };
        solicitud.IniciarAprobacion(plan, TimeSpan.FromHours(48), ahora);
        solicitud.AgregarAdjunto("factura.pdf", "https://rvcuatro.sharepoint.com/factura.pdf", "application/pdf", ahora);

        var idGuardado = solicitud.Id;

        await using (var contextoEscritura = new SolicitudAnticiposDbContext(_opciones))
        {
            contextoEscritura.Solicitudes.Add(solicitud);
            await contextoEscritura.SaveChangesAsync();
        }

        await using var contextoLectura = new SolicitudAnticiposDbContext(_opciones);
        var recuperada = await contextoLectura.Solicitudes.SingleAsync(s => s.Id == idGuardado);

        Assert.Equal(EstadoSolicitud.EnAprobacion, recuperada.Estado);
        Assert.Equal(2, recuperada.Pasos.Count);
        Assert.Single(recuperada.Adjuntos);
        Assert.True(recuperada.Bitacora.Count >= 2); // creada + cadena iniciada, al menos

        var primerPaso = recuperada.Pasos.Single(p => p.Orden == 1);
        Assert.Equal(EstadoPasoAprobacion.Pendiente, primerPaso.Estado);
        Assert.Equal("gerente@rvcuatro.com", primerPaso.AprobadorEmail);
        Assert.NotNull(primerPaso.FechaLimite);
    }

    [Fact]
    public async Task RegistrarRespuesta_persiste_correctamente_tras_recargar_desde_la_base()
    {
        var ahora = DateTimeOffset.UtcNow;
        var solicitud = Domain.Entities.Solicitud.Crear(
            "resp-xyz", "colaborador@rvcuatro.com", "Colaborador", "RVCUATRO", "Corporativo",
            1000m, esUrgente: false, incluyeAnalistaFinanciero: false, ModoAprobacion.Secuencial, ahora);

        solicitud.IniciarAprobacion(
            new List<PlanAprobador> { new(1, 1, "gerente@rvcuatro.com", "Gerente") },
            TimeSpan.FromHours(48), ahora);

        var idGuardado = solicitud.Id;

        await using (var contexto = new SolicitudAnticiposDbContext(_opciones))
        {
            contexto.Solicitudes.Add(solicitud);
            await contexto.SaveChangesAsync();
        }

        // Simula al aprobador entrando al panel web y aprobando: se recarga desde la BD,
        // se aplica la transición de dominio, y se guarda de nuevo.
        await using (var contextoAprobar = new SolicitudAnticiposDbContext(_opciones))
        {
            var solicitudRecargada = await contextoAprobar.Solicitudes.SingleAsync(s => s.Id == idGuardado);
            var token = solicitudRecargada.Pasos.Single().TokenAprobacion;

            solicitudRecargada.RegistrarRespuesta(token, aprobado: true, comentario: "Aprobado", TimeSpan.FromHours(48), ahora.AddHours(1));
            await contextoAprobar.SaveChangesAsync();
        }

        await using var contextoFinal = new SolicitudAnticiposDbContext(_opciones);
        var solicitudFinal = await contextoFinal.Solicitudes.SingleAsync(s => s.Id == idGuardado);

        Assert.Equal(EstadoSolicitud.Aprobada, solicitudFinal.Estado);
        Assert.Equal(EstadoPasoAprobacion.Aprobado, solicitudFinal.Pasos.Single().Estado);
    }
}
