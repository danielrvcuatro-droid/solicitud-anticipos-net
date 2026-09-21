using System.ComponentModel.DataAnnotations;
using SolicitudAnticipos.Domain.Enums;

namespace SolicitudAnticipos.Api.Contratos;

/// <summary>
/// Payload que envía el flujo mínimo de Power Automate (Forms trigger + subida de adjuntos a
/// SharePoint) al endpoint de ingesta. Reemplaza toda la lógica de negocio que antes vivía en el
/// flujo (matrices, validaciones, notificaciones): Automate ahora solo capta el formulario, sube
/// los archivos, y nos entrega estos datos.
/// </summary>
/// <param name="FormsResponseId">Id de la respuesta de Microsoft Forms (permite reintentos idempotentes).</param>
/// <param name="Adjuntos">Archivos ya subidos a SharePoint por el flujo (URL final, no el contenido).</param>
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

/// <summary>Un archivo ya cargado a SharePoint por el flujo de Automate.</summary>
public sealed record AdjuntoRequest(
    [Required] string NombreArchivo,
    [Required, Url] string UrlSharePoint,
    string? TipoContenido);
