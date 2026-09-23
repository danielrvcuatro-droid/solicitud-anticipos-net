using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolicitudAnticipos.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarNumeroSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Arranca en 1000 (decisión del negocio: un sistema nuevo, numeración limpia y con
            // apariencia "oficial" en vez de empezar en 1). La app pide el siguiente valor con
            // nextval() ANTES de crear cada solicitud (ver ISolicitudRepository.ObtenerSiguienteNumeroAsync),
            // así que la secuencia nunca la toca directamente ninguna columna con IDENTITY/serial.
            migrationBuilder.Sql("CREATE SEQUENCE solicitudes_numero_seq START WITH 1000 INCREMENT BY 1;");

            // defaultValueSql (no defaultValue: 0) para que cualquier fila que ya exista en la tabla
            // (como la solicitud de prueba que ya insertamos) se rellene con un número real de la
            // misma secuencia, en vez de quedar con 0 y romper la regla "el número debe ser > 0".
            migrationBuilder.AddColumn<int>(
                name: "numero",
                table: "solicitudes",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('solicitudes_numero_seq')");

            // A partir de aquí, todo INSERT nuevo lo hace la app pasando el número ya generado
            // explícitamente (ObtenerSiguienteNumeroAsync); quitamos el default de la columna para
            // que quede claro que ya no depende de él.
            migrationBuilder.Sql("ALTER TABLE solicitudes ALTER COLUMN numero DROP DEFAULT;");

            migrationBuilder.CreateIndex(
                name: "ix_solicitudes_numero",
                table: "solicitudes",
                column: "numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_solicitudes_numero",
                table: "solicitudes");

            migrationBuilder.DropColumn(
                name: "numero",
                table: "solicitudes");

            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS solicitudes_numero_seq;");
        }
    }
}
