using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sistema_Escolar.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Nota",
                table: "CalificacionBloques",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Peso",
                table: "BloqueEvaluaciones",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAsignacion",
                table: "BloqueEvaluaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BloqueFechas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BloqueEvaluacionId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloqueFechas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloqueFechas_BloqueEvaluaciones_BloqueEvaluacionId",
                        column: x => x.BloqueEvaluacionId,
                        principalTable: "BloqueEvaluaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentosEvaluacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentosEvaluacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AsistenciaBloques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalificacionBloqueId = table.Column<int>(type: "int", nullable: false),
                    BloqueFechaId = table.Column<int>(type: "int", nullable: false),
                    Asistio = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsistenciaBloques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsistenciaBloques_BloqueFechas_BloqueFechaId",
                        column: x => x.BloqueFechaId,
                        principalTable: "BloqueFechas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AsistenciaBloques_CalificacionBloques_CalificacionBloqueId",
                        column: x => x.CalificacionBloqueId,
                        principalTable: "CalificacionBloques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Asistencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentoEvaluacionId = table.Column<int>(type: "int", nullable: false),
                    MatriculaId = table.Column<int>(type: "int", nullable: false),
                    Presente = table.Column<bool>(type: "bit", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asistencias_InstrumentosEvaluacion_InstrumentoEvaluacionId",
                        column: x => x.InstrumentoEvaluacionId,
                        principalTable: "InstrumentosEvaluacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cuatrimestres",
                columns: new[] { "Id", "Nombre", "Numero" },
                values: new object[,]
                {
                    { 1, "Cuatrimestre1", 1 },
                    { 2, "Cuatrimestre2", 2 }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Administrador" },
                    { 2, "Docente" },
                    { 3, "Estudiante" }
                });

            migrationBuilder.InsertData(
                table: "Cursos",
                columns: new[] { "Id", "Codigo", "CreadoPorId", "Creditos", "CuatrimestreId", "Descripcion", "FechaCreacion", "FechaModificacion", "ModificadoPorId", "Nombre" },
                values: new object[,]
                {
                    { 1, "C001", null, 3, 1, "Curso de matemática", new DateTime(2025, 11, 24, 20, 20, 47, 776, DateTimeKind.Utc).AddTicks(3740), null, null, "Matemáticas Básicas" },
                    { 2, "C002", null, 4, 1, "Curso de programación", new DateTime(2025, 11, 24, 20, 20, 47, 776, DateTimeKind.Utc).AddTicks(3750), null, null, "Introducción a la Programación" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsistenciaBloques_BloqueFechaId",
                table: "AsistenciaBloques",
                column: "BloqueFechaId");

            migrationBuilder.CreateIndex(
                name: "IX_AsistenciaBloques_CalificacionBloqueId",
                table: "AsistenciaBloques",
                column: "CalificacionBloqueId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_InstrumentoEvaluacionId",
                table: "Asistencias",
                column: "InstrumentoEvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_BloqueFechas_BloqueEvaluacionId",
                table: "BloqueFechas",
                column: "BloqueEvaluacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsistenciaBloques");

            migrationBuilder.DropTable(
                name: "Asistencias");

            migrationBuilder.DropTable(
                name: "BloqueFechas");

            migrationBuilder.DropTable(
                name: "InstrumentosEvaluacion");

            migrationBuilder.DeleteData(
                table: "Cuatrimestres",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cursos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cursos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Cuatrimestres",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "FechaAsignacion",
                table: "BloqueEvaluaciones");

            migrationBuilder.AlterColumn<decimal>(
                name: "Nota",
                table: "CalificacionBloques",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Peso",
                table: "BloqueEvaluaciones",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
