using SolicitudAnticipos.Domain.Enums;
using SolicitudAnticipos.Domain.Excepciones;

namespace SolicitudAnticipos.Domain.Entities;

/// <summary>
/// Representa a un aprobador dentro de la cadena de aprobación de una <see cref="Solicitud"/>.
/// Es parte del agregado Solicitud: solo la propia Solicitud modifica su estado,
/// para que las reglas de transición vivan en un único lugar.
/// </summary>
public sealed class PasoAprobacion
{
    public Guid Id { get; private set; }
    public Guid SolicitudId { get; private set; }

    /// <summary>Orden absoluto dentro de la cadena (para mostrar la secuencia completa).</summary>
    public int Orden { get; private set; }

    /// <summary>
    /// Nivel de aprobación (los pasos del mismo nivel se activan juntos cuando el modo es Paralelo,
    /// o uno a la vez respetando <see cref="Orden"/> cuando el modo es Secuencial).
    /// </summary>
    public int Nivel { get; private set; }

    public string AprobadorEmail { get; private set; }
    public string AprobadorNombre { get; private set; }

    public EstadoPasoAprobacion Estado { get; private set; }

    /// <summary>Token de un solo uso incluido en el enlace del correo / panel web de aprobación.</summary>
    public string TokenAprobacion { get; private set; }

    public DateTimeOffset? FechaNotificacion { get; private set; }
    public DateTimeOffset? FechaLimite { get; private set; }
    public DateTimeOffset? FechaRespuesta { get; private set; }
    public string? Comentario { get; private set; }

    private PasoAprobacion(
        Guid id,
        Guid solicitudId,
        int orden,
        int nivel,
        string aprobadorEmail,
        string aprobadorNombre)
    {
        Id = id;
        SolicitudId = solicitudId;
        Orden = orden;
        Nivel = nivel;
        AprobadorEmail = aprobadorEmail;
        AprobadorNombre = aprobadorNombre;
        Estado = EstadoPasoAprobacion.Programado;
        TokenAprobacion = Guid.NewGuid().ToString("N");
    }

    internal static PasoAprobacion Crear(Guid solicitudId, int orden, int nivel, string aprobadorEmail, string aprobadorNombre)
    {
        if (string.IsNullOrWhiteSpace(aprobadorEmail))
        {
            throw new SolicitudDomainException("El correo del aprobador es requerido para crear un paso de aprobación.");
        }

        return new PasoAprobacion(Guid.NewGuid(), solicitudId, orden, nivel, aprobadorEmail, aprobadorNombre);
    }

    internal void Notificar(DateTimeOffset ahora, TimeSpan plazo)
    {
        if (Estado != EstadoPasoAprobacion.Programado)
        {
            throw new SolicitudDomainException($"No se puede notificar un paso en estado {Estado}.");
        }

        Estado = EstadoPasoAprobacion.Pendiente;
        FechaNotificacion = ahora;
        FechaLimite = ahora.Add(plazo);
    }

    internal void Aprobar(DateTimeOffset ahora, string? comentario)
    {
        AsegurarPendiente();
        Estado = EstadoPasoAprobacion.Aprobado;
        FechaRespuesta = ahora;
        Comentario = comentario;
    }

    internal void Rechazar(DateTimeOffset ahora, string? comentario)
    {
        AsegurarPendiente();
        Estado = EstadoPasoAprobacion.Rechazado;
        FechaRespuesta = ahora;
        Comentario = comentario;
    }

    internal void MarcarVencido(DateTimeOffset ahora)
    {
        AsegurarPendiente();
        Estado = EstadoPasoAprobacion.Vencido;
        FechaRespuesta = ahora;
    }

    internal void Omitir()
    {
        if (Estado is EstadoPasoAprobacion.Programado or EstadoPasoAprobacion.Pendiente)
        {
            Estado = EstadoPasoAprobacion.Omitido;
        }
    }

    private void AsegurarPendiente()
    {
        if (Estado != EstadoPasoAprobacion.Pendiente)
        {
            throw new SolicitudDomainException(
                $"El paso de {AprobadorEmail} no está pendiente de respuesta (estado actual: {Estado}).");
        }
    }
}
