using Microsoft.EntityFrameworkCore;
using SolicitudAnticipos.Domain.Entities;

namespace SolicitudAnticipos.Infrastructure.Persistencia;

public sealed class SolicitudAnticiposDbContext : DbContext
{
    public SolicitudAnticiposDbContext(DbContextOptions<SolicitudAnticiposDbContext> options) : base(options)
    {
    }

    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    public DbSet<MatrizAprobacion> MatricesAprobacion => Set<MatrizAprobacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SolicitudAnticiposDbContext).Assembly);

        AplicarConvencionSnakeCase(modelBuilder);
    }

    /// <summary>
    /// Renombra tablas, columnas, llaves e índices a snake_case para que el esquema en
    /// Supabase se vea idiomático para Postgres, sin tener que escribir <c>HasColumnName</c>
    /// a mano en cada propiedad.
    /// </summary>
    private static void AplicarConvencionSnakeCase(ModelBuilder modelBuilder)
    {
        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
        {
            entidad.SetTableName(ConvertidorSnakeCase.Convertir(entidad.GetTableName()!));

            foreach (var propiedad in entidad.GetProperties())
            {
                propiedad.SetColumnName(ConvertidorSnakeCase.Convertir(propiedad.GetColumnName()));
            }

            foreach (var llave in entidad.GetKeys())
            {
                var nombre = llave.GetName();
                if (nombre is not null)
                {
                    llave.SetName(ConvertidorSnakeCase.Convertir(nombre));
                }
            }

            foreach (var llaveForanea in entidad.GetForeignKeys())
            {
                var nombre = llaveForanea.GetConstraintName();
                if (nombre is not null)
                {
                    llaveForanea.SetConstraintName(ConvertidorSnakeCase.Convertir(nombre));
                }
            }

            foreach (var indice in entidad.GetIndexes())
            {
                var nombre = indice.GetDatabaseName();
                if (nombre is not null)
                {
                    indice.SetDatabaseName(ConvertidorSnakeCase.Convertir(nombre));
                }
            }
        }
    }
}
