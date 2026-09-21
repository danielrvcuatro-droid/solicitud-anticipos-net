using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolicitudAnticipos.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "matrices_aprobacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    departamento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nivel = table.Column<int>(type: "integer", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    aprobador_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    aprobador_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    es_analista_financiero = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_matrices_aprobacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "solicitudes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    forms_response_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    solicitante_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    solicitante_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sociedad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    departamento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    es_urgente = table.Column<bool>(type: "boolean", nullable: false),
                    incluye_analista_financiero = table.Column<bool>(type: "boolean", nullable: false),
                    modo_aprobacion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    comentario_resolucion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    nombre_resolutor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitudes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "adjuntos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    solicitud_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_archivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    url_share_point = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    tipo_contenido = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    fecha_carga = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_adjuntos", x => x.id);
                    table.ForeignKey(
                        name: "fk_adjuntos_solicitudes_solicitud_id",
                        column: x => x.solicitud_id,
                        principalTable: "solicitudes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bitacora_eventos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    solicitud_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    detalle = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bitacora_eventos", x => x.id);
                    table.ForeignKey(
                        name: "fk_bitacora_eventos_solicitudes_solicitud_id",
                        column: x => x.solicitud_id,
                        principalTable: "solicitudes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pasos_aprobacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    solicitud_id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    nivel = table.Column<int>(type: "integer", nullable: false),
                    aprobador_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    aprobador_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    token_aprobacion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    fecha_notificacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fecha_limite = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fecha_respuesta = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    comentario = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pasos_aprobacion", x => x.id);
                    table.ForeignKey(
                        name: "fk_pasos_aprobacion_solicitudes_solicitud_id",
                        column: x => x.solicitud_id,
                        principalTable: "solicitudes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_adjuntos_solicitud_id",
                table: "adjuntos",
                column: "solicitud_id");

            migrationBuilder.CreateIndex(
                name: "ix_bitacora_eventos_solicitud_id",
                table: "bitacora_eventos",
                column: "solicitud_id");

            migrationBuilder.CreateIndex(
                name: "ix_matrices_aprobacion_departamento_activo",
                table: "matrices_aprobacion",
                columns: new[] { "departamento", "activo" });

            migrationBuilder.CreateIndex(
                name: "ix_pasos_aprobacion_solicitud_id",
                table: "pasos_aprobacion",
                column: "solicitud_id");

            migrationBuilder.CreateIndex(
                name: "ix_pasos_aprobacion_token_aprobacion",
                table: "pasos_aprobacion",
                column: "token_aprobacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitudes_forms_response_id",
                table: "solicitudes",
                column: "forms_response_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adjuntos");

            migrationBuilder.DropTable(
                name: "bitacora_eventos");

            migrationBuilder.DropTable(
                name: "matrices_aprobacion");

            migrationBuilder.DropTable(
                name: "pasos_aprobacion");

            migrationBuilder.DropTable(
                name: "solicitudes");
        }
    }
}
