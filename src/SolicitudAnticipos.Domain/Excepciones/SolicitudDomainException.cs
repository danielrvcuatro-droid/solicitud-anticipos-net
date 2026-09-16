namespace SolicitudAnticipos.Domain.Excepciones;

/// <summary>
/// Se lanza cuando se intenta una transición de estado inválida sobre una <see cref="Entities.Solicitud"/>
/// o sus pasos de aprobación (por ejemplo, responder un paso que ya no está pendiente).
/// </summary>
public sealed class SolicitudDomainException : Exception
{
    public SolicitudDomainException(string message) : base(message)
    {
    }
}
