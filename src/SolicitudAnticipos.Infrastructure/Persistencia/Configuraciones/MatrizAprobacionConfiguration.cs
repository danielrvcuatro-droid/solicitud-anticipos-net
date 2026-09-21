using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolicitudAnticipos.Domain.Entities;

namespace SolicitudAnticipos.Infrastructure.Persistencia.Configuraciones;

public sealed class MatrizAprobacionConfiguration : IEntityTypeConfiguration<MatrizAprobacion>
{
    public void Configure(EntityTypeBuilder<MatrizAprobacion> builder)
    {
        builder.ToTable("matrices_aprobacion");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Departamento).IsRequired().HasMaxLength(100);
        builder.Property(m => m.AprobadorEmail).IsRequired().HasMaxLength(320);
        builder.Property(m => m.AprobadorNombre).HasMaxLength(200);

        builder.HasIndex(m => new { m.Departamento, m.Activo });
    }
}
