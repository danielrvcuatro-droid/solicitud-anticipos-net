namespace SolicitudAnticipos.Domain.Entities;

/// <summary>
/// Una fila configurable de la matriz de aprobación: quién aprueba, en qué orden/nivel,
/// para un departamento dado. Reemplaza las matrices "quemadas" dentro del flujo de Power
/// Automate (matrizAprobacionesCorporativo, matrizAprobacionesDesarrollo, etc.) por filas
/// editables desde base de datos.
/// </summary>
public sealed class MatrizAprobacion
{
    public Guid Id { get; private set; }

    /// <summary>Departamento al que aplica esta fila (debe calzar con <see cref="Solicitud.Departamento"/>).</summary>
    public string Departamento { get; private set; }

    /// <summary>Nivel de aprobación (ver <see cref="Enums.ModoAprobacion"/> para cómo se agrupan).</summary>
    public int Nivel { get; private set; }

    /// <summary>Orden dentro del nivel, usado cuando el modo de ese nivel es secuencial.</summary>
    public int Orden { get; private set; }

    public string AprobadorEmail { get; private set; }
    public string AprobadorNombre { get; private set; }

    /// <summary>
    /// Marca esta fila como el paso del analista financiero: solo se incluye en el plan
    /// cuando la solicitud lo requiere (ver <see cref="Politicas.ConstructorPlanAprobacion"/>),
    /// en vez de estar siempre presente para todos los departamentos.
    /// </summary>
    public bool EsAnalistaFinanciero { get; private set; }

    /// <summary>Permite desactivar una fila (p. ej. alguien dejó la empresa) sin borrar el histórico.</summary>
    public bool Activo { get; private set; }

    private MatrizAprobacion(
        Guid id,
        string departamento,
        int nivel,
        int orden,
        string aprobadorEmail,
        string aprobadorNombre,
        bool esAnalistaFinanciero)
    {
        Id = id;
        Departamento = departamento;
        Nivel = nivel;
        Orden = orden;
        AprobadorEmail = aprobadorEmail;
        AprobadorNombre = aprobadorNombre;
        EsAnalistaFinanciero = esAnalistaFinanciero;
        Activo = true;
    }

    public static MatrizAprobacion Crear(
        string departamento,
        int nivel,
        int orden,
        string aprobadorEmail,
        string aprobadorNombre,
        bool esAnalistaFinanciero = false)
    {
        if (string.IsNullOrWhiteSpace(departamento))
        {
            throw new ArgumentException("El departamento es requerido.", nameof(departamento));
        }

        if (string.IsNullOrWhiteSpace(aprobadorEmail))
        {
            throw new ArgumentException("El correo del aprobador es requerido.", nameof(aprobadorEmail));
        }

        return new MatrizAprobacion(Guid.NewGuid(), departamento, nivel, orden, aprobadorEmail, aprobadorNombre, esAnalistaFinanciero);
    }

    public void Desactivar() => Activo = false;

    public void Activar() => Activo = true;

    public void ActualizarAprobador(string aprobadorEmail, string aprobadorNombre)
    {
        if (string.IsNullOrWhiteSpace(aprobadorEmail))
        {
            throw new ArgumentException("El correo del aprobador es requerido.", nameof(aprobadorEmail));
        }

        AprobadorEmail = aprobadorEmail;
        AprobadorNombre = aprobadorNombre;
    }
}
