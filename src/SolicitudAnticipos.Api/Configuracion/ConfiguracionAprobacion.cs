namespace SolicitudAnticipos.Api.Configuracion;

/// <summary>
/// Parámetros del flujo de aprobación configurables por <c>appsettings</c> en vez de estar
/// quemados en el código (el plazo por paso, antes fijo en el flujo de Power Automate).
/// </summary>
public sealed class ConfiguracionAprobacion
{
    public const string Seccion = "Aprobacion";

    /// <summary>Horas que tiene cada aprobador para responder antes de que su paso se marque como vencido.</summary>
    public int PlazoPorPasoHoras { get; set; } = 48;
}
