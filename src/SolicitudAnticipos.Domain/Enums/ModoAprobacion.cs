namespace SolicitudAnticipos.Domain.Enums;

/// <summary>
/// Cómo se activan los pasos que comparten el mismo nivel dentro de la cadena de aprobación.
/// Mejora respecto al flujo original (que era siempre secuencial, aprobador por aprobador).
/// </summary>
public enum ModoAprobacion
{
    /// <summary>Los aprobadores del mismo nivel se notifican uno a la vez, en el orden definido.</summary>
    Secuencial = 0,

    /// <summary>Los aprobadores del mismo nivel se notifican todos al mismo tiempo.</summary>
    Paralelo = 1
}
