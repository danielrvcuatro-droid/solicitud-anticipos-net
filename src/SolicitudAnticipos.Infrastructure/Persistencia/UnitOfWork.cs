using SolicitudAnticipos.Domain.Abstracciones;

namespace SolicitudAnticipos.Infrastructure.Persistencia;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SolicitudAnticiposDbContext _dbContext;

    public UnitOfWork(SolicitudAnticiposDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
