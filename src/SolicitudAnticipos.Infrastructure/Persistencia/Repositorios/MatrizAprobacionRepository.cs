using Microsoft.EntityFrameworkCore;
using SolicitudAnticipos.Domain.Abstracciones;
using SolicitudAnticipos.Domain.Entities;

namespace SolicitudAnticipos.Infrastructure.Persistencia.Repositorios;

public sealed class MatrizAprobacionRepository : IMatrizAprobacionRepository
{
    private readonly SolicitudAnticiposDbContext _dbContext;

    public MatrizAprobacionRepository(SolicitudAnticiposDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MatrizAprobacion>> ObtenerPorDepartamentoAsync(string departamento, CancellationToken cancellationToken = default) =>
        await _dbContext.MatricesAprobacion
            .Where(m => m.Departamento == departamento)
            .OrderBy(m => m.Nivel)
            .ThenBy(m => m.Orden)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MatrizAprobacion>> ObtenerTodasAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.MatricesAprobacion
            .OrderBy(m => m.Departamento)
            .ThenBy(m => m.Nivel)
            .ThenBy(m => m.Orden)
            .ToListAsync(cancellationToken);

    public void Agregar(MatrizAprobacion fila) => _dbContext.MatricesAprobacion.Add(fila);
}
