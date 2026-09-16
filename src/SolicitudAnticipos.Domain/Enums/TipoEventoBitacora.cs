namespace SolicitudAnticipos.Domain.Enums;

/// <summary>
/// Tipos de evento que se registran en la bitácora de auditoría de una solicitud.
/// </summary>
public enum TipoEventoBitacora
{
    SolicitudCreada,
    CadenaAprobacionIniciada,
    PasoNotificado,
    PasoAprobado,
    PasoRechazado,
    PasoVencido,
    NivelCompletado,
    SolicitudAprobada,
    SolicitudRechazada,
    SolicitudVencida,
    RecordatorioEnviado,
    ErrorNotificacion
}
