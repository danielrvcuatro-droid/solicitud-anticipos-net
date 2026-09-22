using SolicitudAnticipos.Domain.Enums;
using SolicitudAnticipos.Domain.Excepciones;
using SolicitudAnticipos.Domain.Politicas;

namespace SolicitudAnticipos.Domain.Entities;

/// <summary>
/// Agregado raíz: una solicitud de anticipo y toda su cadena de aprobación.
/// Concentra las reglas que en el flujo de Power Automate estaban repartidas entre
/// condiciones (stopFlow, timedOut, validarSiurgenteConsba, incluirAnalistaFinanciero, etc.).
/// </summary>
public sealed class Solicitud
{
    private readonly List<PasoAprobacion> _pasos = new();
    private readonly List<Adjunto> _adjuntos = new();
    private readonly List<EventoBitacora> _bitacora = new();

    public Guid Id { get; private set; }

    /// <summary>
    /// Número secuencial y legible de la solicitud (distinto del <see cref="Id"/>, que es un GUID
    /// interno). Se usa para mostrarlo a las personas y para nombrar la carpeta de adjuntos en
    /// SharePoint (p. ej. "Documentos - No. 1006"), generado por una secuencia de Postgres antes
    /// de crear la solicitud (ver <see cref="Abstracciones.ISolicitudRepository.ObtenerSiguienteNumeroAsync"/>).
    /// </summary>
    public int Numero { get; private set; }

    /// <summary>Id de la respuesta de Microsoft Forms que originó la solicitud (trazabilidad con el origen).</summary>
    public string FormsResponseId { get; private set; }

    public string SolicitanteEmail { get; private set; }
    public string SolicitanteNombre { get; private set; }
    public string Sociedad { get; private set; }
    public string Departamento { get; private set; }
    public decimal Monto { get; private set; }
    public bool EsUrgente { get; private set; }
    public bool IncluyeAnalistaFinanciero { get; private set; }
    public ModoAprobacion ModoAprobacion { get; private set; }

    public EstadoSolicitud Estado { get; private set; }
    public string? ComentarioResolucion { get; private set; }
    public string? NombreResolutor { get; private set; }

    public DateTimeOffset FechaCreacion { get; private set; }
    public DateTimeOffset FechaActualizacion { get; private set; }

    public IReadOnlyCollection<PasoAprobacion> Pasos => _pasos.AsReadOnly();
    public IReadOnlyCollection<Adjunto> Adjuntos => _adjuntos.AsReadOnly();
    public IReadOnlyCollection<EventoBitacora> Bitacora => _bitacora.AsReadOnly();

    private Solicitud()
    {
        // Requerido por EF Core / mapeo desde infraestructura.
        FormsResponseId = string.Empty;
        SolicitanteEmail = string.Empty;
        SolicitanteNombre = string.Empty;
        Sociedad = string.Empty;
        Departamento = string.Empty;
    }

    public static Solicitud Crear(
        int numero,
        string formsResponseId,
        string solicitanteEmail,
        string solicitanteNombre,
        string sociedad,
        string departamento,
        decimal monto,
        bool esUrgente,
        bool incluyeAnalistaFinanciero,
        ModoAprobacion modoAprobacion,
        DateTimeOffset ahora)
    {
        if (numero <= 0)
        {
            throw new SolicitudDomainException("El número de solicitud debe ser mayor a cero.");
        }

        if (string.IsNullOrWhiteSpace(formsResponseId))
        {
            throw new SolicitudDomainException("FormsResponseId es requerido (trazabilidad con el formulario de origen).");
        }

        if (string.IsNullOrWhiteSpace(solicitanteEmail))
        {
            throw new SolicitudDomainException("El correo del solicitante es requerido.");
        }

        if (monto <= 0)
        {
            throw new SolicitudDomainException("El monto de la solicitud debe ser mayor a cero.");
        }

        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            FormsResponseId = formsResponseId,
            SolicitanteEmail = solicitanteEmail,
            SolicitanteNombre = solicitanteNombre,
            Sociedad = sociedad,
            Departamento = departamento,
            Monto = monto,
            EsUrgente = esUrgente,
            IncluyeAnalistaFinanciero = incluyeAnalistaFinanciero,
            ModoAprobacion = modoAprobacion,
            Estado = EstadoSolicitud.Pendiente,
            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };

        solicitud.RegistrarEvento(TipoEventoBitacora.SolicitudCreada,
            $"Solicitud creada por {solicitanteEmail} ({departamento}/{sociedad}) por Q{monto:N2}.", ahora);

        return solicitud;
    }

    public void AgregarAdjunto(string nombreArchivo, string urlSharePoint, string tipoContenido, DateTimeOffset ahora)
    {
        _adjuntos.Add(Adjunto.Crear(Id, nombreArchivo, urlSharePoint, tipoContenido, ahora));
        Tocar(ahora);
    }

    /// <summary>
    /// Arma la cadena de aprobación a partir del plan ya resuelto (matriz de departamento/sociedad,
    /// más el analista financiero si aplica) y notifica el/los primer(os) paso(s) según el modo de aprobación.
    /// </summary>
    public void IniciarAprobacion(IReadOnlyList<PlanAprobador> plan, TimeSpan plazoPorPaso, DateTimeOffset ahora)
    {
        if (Estado != EstadoSolicitud.Pendiente)
        {
            throw new SolicitudDomainException($"No se puede iniciar la aprobación de una solicitud en estado {Estado}.");
        }

        if (plan.Count == 0)
        {
            throw new SolicitudDomainException("El plan de aprobación no puede estar vacío.");
        }

        foreach (var item in plan.OrderBy(p => p.Orden))
        {
            _pasos.Add(PasoAprobacion.Crear(Id, item.Orden, item.Nivel, item.AprobadorEmail, item.AprobadorNombre));
        }

        Estado = EstadoSolicitud.EnAprobacion;
        RegistrarEvento(TipoEventoBitacora.CadenaAprobacionIniciada,
            $"Cadena de aprobación iniciada con {plan.Count} paso(s) en modo {ModoAprobacion}.", ahora);

        NotificarNivelActual(plazoPorPaso, ahora);
    }

    /// <summary>Registra la respuesta (aprobar/rechazar) de un aprobador identificado por su token de un solo uso.</summary>
    public void RegistrarRespuesta(string tokenAprobacion, bool aprobado, string? comentario, TimeSpan plazoPorPaso, DateTimeOffset ahora)
    {
        AsegurarEnAprobacion();

        var paso = _pasos.SingleOrDefault(p => p.TokenAprobacion == tokenAprobacion)
            ?? throw new SolicitudDomainException("El enlace de aprobación no corresponde a ningún paso de esta solicitud.");

        if (aprobado)
        {
            paso.Aprobar(ahora, comentario);
            RegistrarEvento(TipoEventoBitacora.PasoAprobado, $"{paso.AprobadorEmail} aprobó el paso {paso.Orden}.", ahora);
        }
        else
        {
            paso.Rechazar(ahora, comentario);
            RegistrarEvento(TipoEventoBitacora.PasoRechazado, $"{paso.AprobadorEmail} rechazó el paso {paso.Orden}.", ahora);
            Rechazar(paso.AprobadorNombre, comentario, ahora);
            return;
        }

        ContinuarFlujo(plazoPorPaso, ahora);
    }

    /// <summary>Marca como vencido el paso indicado (invocado por el worker cuando pasa la fecha límite).</summary>
    public void MarcarPasoVencido(Guid pasoId, DateTimeOffset ahora)
    {
        AsegurarEnAprobacion();

        var paso = _pasos.SingleOrDefault(p => p.Id == pasoId)
            ?? throw new SolicitudDomainException("El paso indicado no pertenece a esta solicitud.");

        paso.MarcarVencido(ahora);
        RegistrarEvento(TipoEventoBitacora.PasoVencido, $"El paso {paso.Orden} ({paso.AprobadorEmail}) venció sin respuesta.", ahora);

        foreach (var restante in _pasos.Where(p => p.Estado is EstadoPasoAprobacion.Programado or EstadoPasoAprobacion.Pendiente))
        {
            restante.Omitir();
        }

        Estado = EstadoSolicitud.Vencida;
        RegistrarEvento(TipoEventoBitacora.SolicitudVencida, "La solicitud venció por falta de respuesta de un aprobador.", ahora);
        Tocar(ahora);
    }

    /// <summary>Pasos que en este momento deben notificarse (recién activados por <see cref="NotificarNivelActual"/>).</summary>
    public IReadOnlyList<PasoAprobacion> ObtenerPasosPendientesDeNotificar() =>
        _pasos.Where(p => p.Estado == EstadoPasoAprobacion.Pendiente && p.FechaNotificacion is not null).ToList();

    private void ContinuarFlujo(TimeSpan plazoPorPaso, DateTimeOffset ahora)
    {
        var nivelActual = _pasos.Where(p => p.Estado != EstadoPasoAprobacion.Omitido).Min(p => p.Nivel);
        var pasosNivelActual = _pasos.Where(p => p.Nivel == nivelActual).ToList();

        var completo = pasosNivelActual.All(p => p.Estado == EstadoPasoAprobacion.Aprobado);
        if (!completo)
        {
            // Modo secuencial dentro del mismo nivel: activar el siguiente paso programado.
            Tocar(ahora);
            NotificarNivelActual(plazoPorPaso, ahora);
            return;
        }

        RegistrarEvento(TipoEventoBitacora.NivelCompletado, $"Nivel {nivelActual} completado, todos aprobaron.", ahora);

        var quedanNiveles = _pasos.Any(p => p.Nivel > nivelActual && p.Estado == EstadoPasoAprobacion.Programado);
        if (quedanNiveles)
        {
            NotificarNivelActual(plazoPorPaso, ahora);
            Tocar(ahora);
            return;
        }

        Estado = EstadoSolicitud.Aprobada;
        RegistrarEvento(TipoEventoBitacora.SolicitudAprobada, "Todos los niveles de aprobación fueron aprobados.", ahora);
        Tocar(ahora);
    }

    private void NotificarNivelActual(TimeSpan plazoPorPaso, DateTimeOffset ahora)
    {
        var siguienteNivelPendiente = _pasos
            .Where(p => p.Estado == EstadoPasoAprobacion.Programado)
            .Select(p => (int?)p.Nivel)
            .DefaultIfEmpty(null)
            .Min();

        if (siguienteNivelPendiente is null)
        {
            return;
        }

        var candidatos = _pasos
            .Where(p => p.Nivel == siguienteNivelPendiente && p.Estado == EstadoPasoAprobacion.Programado)
            .OrderBy(p => p.Orden)
            .ToList();

        var aNotificar = ModoAprobacion == ModoAprobacion.Paralelo
            ? candidatos
            : candidatos.Take(1).ToList();

        foreach (var paso in aNotificar)
        {
            paso.Notificar(ahora, plazoPorPaso);
            RegistrarEvento(TipoEventoBitacora.PasoNotificado, $"Se notificó a {paso.AprobadorEmail} (paso {paso.Orden}).", ahora);
        }
    }

    private void Rechazar(string? nombreRechazador, string? comentario, DateTimeOffset ahora)
    {
        foreach (var restante in _pasos.Where(p => p.Estado is EstadoPasoAprobacion.Programado or EstadoPasoAprobacion.Pendiente))
        {
            restante.Omitir();
        }

        Estado = EstadoSolicitud.Rechazada;
        NombreResolutor = nombreRechazador;
        ComentarioResolucion = comentario;
        RegistrarEvento(TipoEventoBitacora.SolicitudRechazada, $"Solicitud rechazada por {nombreRechazador}.", ahora);
        Tocar(ahora);
    }

    private void AsegurarEnAprobacion()
    {
        if (Estado != EstadoSolicitud.EnAprobacion)
        {
            throw new SolicitudDomainException($"La solicitud no está en aprobación (estado actual: {Estado}).");
        }
    }

    private void RegistrarEvento(TipoEventoBitacora tipo, string detalle, DateTimeOffset ahora)
    {
        _bitacora.Add(EventoBitacora.Crear(Id, tipo, detalle, ahora));
    }

    private void Tocar(DateTimeOffset ahora) => FechaActualizacion = ahora;
}
