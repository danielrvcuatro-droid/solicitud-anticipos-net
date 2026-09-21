namespace SolicitudAnticipos.Domain.Abstracciones;

/// <summary>
/// Confirma en una sola transacción los cambios hechos a través de los repositorios.
/// La implementación en Infrastructure es, en la práctica, <c>DbContext.SaveChangesAsync</c>.
/// </summary>
public interface IUnitOfWork
{
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
