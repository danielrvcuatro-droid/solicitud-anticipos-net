namespace SolicitudAnticipos.Domain.Enums;

/// <summary>
/// Estado de un paso individual (un aprobador) dentro de la cadena de aprobación.
/// </summary>
public enum EstadoPasoAprobacion
{
    /// <summary>Está en la matriz pero todavía no se le ha notificado (esperando su nivel/turno).</summary>
    Programado = 0,

    /// <summary>Se notificó al aprobador y se espera su respuesta.</summary>
    Pendiente = 1,

    /// <summary>El aprobador aprobó este paso.</summary>
    Aprobado = 2,

    /// <summary>El aprobador rechazó este paso.</summary>
    Rechazado = 3,

    /// <summary>Venció el plazo sin respuesta del aprobador.</summary>
    Vencido = 4,

    /// <summary>La solicitud terminó (aprobada, rechazada o vencida) antes de llegar a este paso.</summary>
    Omitido = 5
}
