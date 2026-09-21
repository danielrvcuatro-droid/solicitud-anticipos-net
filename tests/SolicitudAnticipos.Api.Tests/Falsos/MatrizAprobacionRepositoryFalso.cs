using SolicitudAnticipos.Domain.Abstracciones;
using SolicitudAnticipos.Domain.Entities;

namespace SolicitudAnticipos.Api.Tests.Falsos;

internal sealed class MatrizAprobacionRepositoryFalso : IMatrizAprobacionRepository
{
    private readonly List<MatrizAprobacion> _filas;

    public MatrizAprobacionRepositoryFalso(IEnumerable<MatrizAprobacion>? filas = null)
    {
        _filas = filas?.ToList() ?? new List<MatrizAprobacion>();
    }

    public Task<IReadOnlyList<MatrizAprobacion>> ObtenerPorDepartamentoAsync(string departamento, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<MatrizAprobacion>>(_filas.Where(f => f.Departamento == departamento).ToList());

    public Task<IReadOnlyList<MatrizAprobacion>> ObtenerTodasAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<MatrizAprobacion>>(_filas);

    public void Agregar(MatrizAprobacion fila) => _filas.Add(fila);
}
