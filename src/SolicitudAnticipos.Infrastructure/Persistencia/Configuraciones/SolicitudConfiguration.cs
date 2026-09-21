using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolicitudAnticipos.Domain.Entities;

namespace SolicitudAnticipos.Infrastructure.Persistencia.Configuraciones;

public sealed class SolicitudConfiguration : IEntityTypeConfiguration<Solicitud>
{
    public void Configure(EntityTypeBuilder<Solicitud> builder)
    {
        builder.ToTable("solicitudes");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.FormsResponseId).IsRequired().HasMaxLength(200);
        builder.HasIndex(s => s.FormsResponseId).IsUnique();

        builder.Property(s => s.SolicitanteEmail).IsRequired().HasMaxLength(320);
        builder.Property(s => s.SolicitanteNombre).HasMaxLength(200);
        builder.Property(s => s.Sociedad).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Departamento).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Monto).HasPrecision(18, 2);

        builder.Property(s => s.Estado).HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.ModoAprobacion).HasConversion<string>().HasMaxLength(30);

        builder.Property(s => s.ComentarioResolucion).HasMaxLength(2000);
        builder.Property(s => s.NombreResolutor).HasMaxLength(200);

        // El agregado expone Pasos/Adjuntos/Bitacora como colecciones de solo lectura (sin setter);
        // EF Core las mapea igual usando el campo privado que respalda cada propiedad (_pasos, etc.)
        // en vez de pasar por la propiedad pública.
        builder.HasMany(s => s.Pasos)
            .WithOne()
            .HasForeignKey(p => p.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Pasos)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(s => s.Adjuntos)
            .WithOne()
            .HasForeignKey(a => a.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Adjuntos)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(s => s.Bitacora)
            .WithOne()
            .HasForeignKey(e => e.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Bitacora)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}
