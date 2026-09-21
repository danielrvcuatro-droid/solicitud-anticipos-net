using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolicitudAnticipos.Domain.Entities;

namespace SolicitudAnticipos.Infrastructure.Persistencia.Configuraciones;

public sealed class EventoBitacoraConfiguration : IEntityTypeConfiguration<EventoBitacora>
{
    public void Configure(EntityTypeBuilder<EventoBitacora> builder)
    {
        builder.ToTable("bitacora_eventos");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Detalle).IsRequired().HasMaxLength(2000);
    }
}
