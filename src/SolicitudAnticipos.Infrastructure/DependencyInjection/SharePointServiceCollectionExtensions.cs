using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolicitudAnticipos.Domain.Abstracciones;
using SolicitudAnticipos.Infrastructure.SharePoint;

namespace SolicitudAnticipos.Infrastructure.DependencyInjection;

public static class SharePointServiceCollectionExtensions
{
    /// <summary>
    /// Registra el almacenamiento de adjuntos contra SharePoint (vía Microsoft Graph).
    /// Se llama desde Program.cs de la Api: <c>builder.Services.AddSharePoint(builder.Configuration);</c>
    /// Requiere la sección <c>SharePoint</c> en la configuración (ver <see cref="ConfiguracionSharePoint"/>).
    /// </summary>
    public static IServiceCollection AddSharePoint(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConfiguracionSharePoint>(configuration.GetSection(ConfiguracionSharePoint.Seccion));
        services.AddSingleton<IAlmacenamientoAdjuntos, SharePointAlmacenamientoAdjuntos>();

        return services;
    }
}
