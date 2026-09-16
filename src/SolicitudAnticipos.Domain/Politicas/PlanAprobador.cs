namespace SolicitudAnticipos.Domain.Politicas;

/// <summary>
/// Un aprobador ya resuelto a partir de la matriz de aprobación (departamento/sociedad),
/// listo para convertirse en un <see cref="Entities.PasoAprobacion"/>.
/// Lo arma la capa de aplicación/infraestructura consultando la matriz configurable;
/// el dominio solo necesita saber orden, nivel y quién es la persona.
/// </summary>
/// <param name="Orden">Posición absoluta dentro de la cadena completa.</param>
/// <param name="Nivel">Nivel de aprobación al que pertenece (ver <see cref="Enums.ModoAprobacion"/>).</param>
/// <param name="AprobadorEmail">Correo del aprobador, usado también para autenticación por magic link.</param>
/// <param name="AprobadorNombre">Nombre para mostrar en notificaciones y en el panel.</param>
public sealed record PlanAprobador(int Orden, int Nivel, string AprobadorEmail, string AprobadorNombre);
