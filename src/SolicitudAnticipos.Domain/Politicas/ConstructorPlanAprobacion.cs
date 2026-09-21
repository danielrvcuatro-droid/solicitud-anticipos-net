using SolicitudAnticipos.Domain.Entities;
using SolicitudAnticipos.Domain.Excepciones;

namespace SolicitudAnticipos.Domain.Politicas;

/// <summary>
/// Convierte las filas activas de <see cref="MatrizAprobacion"/> de un departamento en el
/// <see cref="PlanAprobador"/> que consume <see cref="Solicitud.IniciarAprobacion"/>.
/// Equivale a lo que en el flujo original resolvían juntas las acciones "Filtrar matriz",
/// "Seleccionar" e "incluirAnalistaFinanciero".
/// </summary>
public static class ConstructorPlanAprobacion
{
    /// <param name="filasMatriz">Filas de la matriz para el departamento de la solicitud (ya filtradas por departamento).</param>
    /// <param name="incluyeAnalistaFinanciero">
    /// Si es false, se excluyen del plan las filas marcadas <see cref="MatrizAprobacion.EsAnalistaFinanciero"/>.
    /// </param>
    public static IReadOnlyList<PlanAprobador> Construir(IEnumerable<MatrizAprobacion> filasMatriz, bool incluyeAnalistaFinanciero)
    {
        var filas = filasMatriz
            .Where(f => f.Activo)
            .Where(f => incluyeAnalistaFinanciero || !f.EsAnalistaFinanciero)
            .OrderBy(f => f.Nivel)
            .ThenBy(f => f.Orden)
            .ToList();

        if (filas.Count == 0)
        {
            throw new SolicitudDomainException(
                "No hay aprobadores activos configurados para este departamento. Revisa la matriz de aprobación.");
        }

        return filas
            .Select((f, indice) => new PlanAprobador(Orden: indice + 1, Nivel: f.Nivel, f.AprobadorEmail, f.AprobadorNombre))
            .ToList();
    }
}
