using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolicitudAnticipos.Domain.Entities;

namespace SolicitudAnticipos.Infrastructure.Persistencia.Configuraciones;

public sealed class AdjuntoConfiguration : IEntityTypeConfiguration<Adjunto>
{
    public void Configure(EntityTypeBuilder<Adjunto> builder)
    {
        builder.ToTable("adjuntos");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.NombreArchivo).IsRequired().HasMaxLength(260);
        builder.Property(a => a.UrlSharePoint).IsRequired().HasMaxLength(2000);
        builder.Property(a => a.TipoContenido).HasMaxLength(150);
    }
}
