namespace SolicitudAnticipos.Domain.Enums;

/// <summary>
/// Estado general de una solicitud de anticipo a lo largo de su ciclo de vida.
/// </summary>
public enum EstadoSolicitud
{
    /// <summary>Fue creada pero aún no se ha armado la cadena de aprobadores.</summary>
    Pendiente = 0,

    /// <summary>Tiene al menos un paso de aprobación activo esperando respuesta.</summary>
    EnAprobacion = 1,

    /// <summary>Todos los pasos requeridos fueron aprobados.</summary>
    Aprobada = 2,

    /// <summary>Un aprobador la rechazó explícitamente; el flujo se detiene.</summary>
    Rechazada = 3,

    /// <summary>Un paso de aprobación venció sin respuesta; el flujo se detiene.</summary>
    Vencida = 4
}
