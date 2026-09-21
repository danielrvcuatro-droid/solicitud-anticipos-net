using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolicitudAnticipos.Domain.Entities;

namespace SolicitudAnticipos.Infrastructure.Persistencia.Configuraciones;

public sealed class PasoAprobacionConfiguration : IEntityTypeConfiguration<PasoAprobacion>
{
    public void Configure(EntityTypeBuilder<PasoAprobacion> builder)
    {
        builder.ToTable("pasos_aprobacion");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.AprobadorEmail).IsRequired().HasMaxLength(320);
        builder.Property(p => p.AprobadorNombre).HasMaxLength(200);
        builder.Property(p => p.Estado).HasConversion<string>().HasMaxLength(30);

        builder.Property(p => p.TokenAprobacion).IsRequired().HasMaxLength(64);
        builder.HasIndex(p => p.TokenAprobacion).IsUnique();

        builder.Property(p => p.Comentario).HasMaxLength(2000);
    }
}
