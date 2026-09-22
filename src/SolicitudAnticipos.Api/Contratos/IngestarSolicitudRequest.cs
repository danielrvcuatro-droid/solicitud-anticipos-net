using System.ComponentModel.DataAnnotations;
using SolicitudAnticipos.Domain.Enums;

namespace SolicitudAnticipos.Api.Contratos;

/// <summary>
/// Payload que envía el flujo mínimo de Power Automate (Forms trigger, sin lógica de negocio) al
/// endpoint de ingesta. Automate solo capta el formulario y reenvía el contenido de los archivos
/// adjuntos (con "Get file content"); es nuestro backend quien los sube a SharePoint vía
/// Microsoft Graph y arma la cadena de aprobación.
/// </summary>
/// <param name="FormsResponseId">Id de la respuesta de Microsoft Forms (permite reintentos idempotentes).</param>
/// <param name="Adjuntos">Archivos del formulario (contenido en base64, aún sin subir a ningún lado).</param>
/// <remarks>
/// Los atributos de validación van directo sobre el parámetro (sin <c>property:</c>) porque
/// ASP.NET Core valida records con constructor primario a través de los parámetros, no de las
/// propiedades generadas; con el target <c>property:</c> los ignora en tiempo de ejecución.
/// </remarks>
public sealed record IngestarSolicitudRequest(
    [Required] string FormsResponseId,
    [Required, EmailAddress] string SolicitanteEmail,
    [Required] string SolicitanteNombre,
    [Required] string Sociedad,
    [Required] string Departamento,
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")] decimal Monto,
    bool EsUrgente,
    bool IncluyeAnalistaFinanciero,
    ModoAprobacion ModoAprobacion,
    IReadOnlyList<AdjuntoRequest>? Adjuntos);

/// <summary>Un archivo del formulario, tal como lo entrega la acción "Get file content" de Automate.</summary>
public sealed record AdjuntoRequest(
    [Required] string NombreArchivo,
    [Required] string ContenidoBase64,
    string? TipoContenido);
