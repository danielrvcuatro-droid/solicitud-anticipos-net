namespace SolicitudAnticipos.Api.Contratos;

/// <summary>Representación mínima de una <see cref="Domain.Entities.Solicitud"/> para las respuestas de la Api.</summary>
public sealed record SolicitudResponse(Guid Id, string FormsResponseId, string Estado, int TotalPasos);
