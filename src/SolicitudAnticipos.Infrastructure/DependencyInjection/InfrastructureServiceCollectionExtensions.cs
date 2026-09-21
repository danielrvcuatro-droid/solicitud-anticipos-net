using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolicitudAnticipos.Domain.Abstracciones;
using SolicitudAnticipos.Infrastructure.Persistencia;
using SolicitudAnticipos.Infrastructure.Persistencia.Repositorios;

namespace SolicitudAnticipos.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registra el DbContext (Postgres/Supabase vía Npgsql) y los repositorios.
    /// Se llama desde Program.cs de la Api, el Web y el Worker:
    /// <c>builder.Services.AddInfrastructure(builder.Configuration);</c>
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cadenaConexion = configuration.GetConnectionString("Supabase")
            ?? throw new InvalidOperationException(
                "Falta la cadena de conexión 'Supabase' en la configuración (ConnectionStrings:Supabase).");

        services.AddDbContext<SolicitudAnticiposDbContext>(opciones =>
            opciones.UseNpgsql(cadenaConexion, npgsql => npgsql.MigrationsAssembly(typeof(SolicitudAnticiposDbContext).Assembly.FullName)));

        services.AddScoped<ISolicitudRepository, SolicitudRepository>();
        services.AddScoped<IMatrizAprobacionRepository, MatrizAprobacionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
