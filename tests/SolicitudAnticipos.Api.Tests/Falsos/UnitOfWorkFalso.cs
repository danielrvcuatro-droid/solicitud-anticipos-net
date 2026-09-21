using SolicitudAnticipos.Domain.Abstracciones;

namespace SolicitudAnticipos.Api.Tests.Falsos;

internal sealed class UnitOfWorkFalso : IUnitOfWork
{
    public int VecesGuardado { get; private set; }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        VecesGuardado++;
        return Task.CompletedTask;
    }
}
