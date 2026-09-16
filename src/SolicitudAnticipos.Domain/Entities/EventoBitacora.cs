using SolicitudAnticipos.Domain.Enums;

namespace SolicitudAnticipos.Domain.Entities;

/// <summary>
/// Un registro inmutable de auditoría: qué pasó, cuándo, y con qué detalle.
/// A diferencia del flujo de Power Automate original, aquí queda rastro completo de cada evento,
/// no solo del resultado final en el PDF.
/// </summary>
public sealed class EventoBitacora
{
    public Guid Id { get; private set; }
    public Guid SolicitudId { get; private set; }
    public TipoEventoBitacora Tipo { get; private set; }
    public string Detalle { get; private set; }
    public DateTimeOffset Fecha { get; private set; }

    private EventoBitacora(Guid id, Guid solicitudId, TipoEventoBitacora tipo, string detalle, DateTimeOffset fecha)
    {
        Id = id;
        SolicitudId = solicitudId;
        Tipo = tipo;
        Detalle = detalle;
        Fecha = fecha;
    }

    internal static EventoBitacora Crear(Guid solicitudId, TipoEventoBitacora tipo, string detalle, DateTimeOffset ahora) =>
        new(Guid.NewGuid(), solicitudId, tipo, detalle, ahora);
}
