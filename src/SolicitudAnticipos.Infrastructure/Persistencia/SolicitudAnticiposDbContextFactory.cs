using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SolicitudAnticipos.Infrastructure.Persistencia;

/// <summary>
/// Fábrica usada solo en tiempo de diseño por las herramientas de EF Core
/// (<c>dotnet ef migrations add</c>, <c>dotnet ef database update</c>) para poder generar
/// migraciones sin depender de que la Api/Web ya estén configuradas.
///
/// La cadena de conexión real de Supabase se toma de la variable de entorno
/// SOLICITUDANTICIPOS_CONNECTIONSTRING; si no está definida, se usa un valor de relleno
/// (nunca se conecta de verdad para generar el SQL de la migración).
/// </summary>
public sealed class SolicitudAnticiposDbContextFactory : IDesignTimeDbContextFactory<SolicitudAnticiposDbContext>
{
    public SolicitudAnticiposDbContext CreateDbContext(string[] args)
    {
        var cadenaConexion = Environment.GetEnvironmentVariable("SOLICITUDANTICIPOS_CONNECTIONSTRING")
            ?? "Host=localhost;Database=solicitud_anticipos_diseno;Username=postgres;Password=postgres";

        var opciones = new DbContextOptionsBuilder<SolicitudAnticiposDbContext>()
            .UseNpgsql(cadenaConexion)
            .Options;

        return new SolicitudAnticiposDbContext(opciones);
    }
}
