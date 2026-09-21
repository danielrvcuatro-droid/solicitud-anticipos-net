using SolicitudAnticipos.Domain.Entities;

namespace SolicitudAnticipos.Domain.Abstracciones;

/// <summary>Puerto hacia la persistencia de la matriz de aprobación configurable.</summary>
public interface IMatrizAprobacionRepository
{
    /// <summary>Filas activas o no de un departamento (el filtrado por <c>Activo</c> lo hace <see cref="Politicas.ConstructorPlanAprobacion"/>).</summary>
    Task<IReadOnlyList<MatrizAprobacion>> ObtenerPorDepartamentoAsync(string departamento, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MatrizAprobacion>> ObtenerTodasAsync(CancellationToken cancellationToken = default);

    void Agregar(MatrizAprobacion fila);
}
