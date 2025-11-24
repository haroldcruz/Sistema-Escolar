using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaEscolar.Data.Migrations
{
 public partial class AddCursoOferta : Migration
 {
 protected override void Up(MigrationBuilder migrationBuilder)
 {
 // Create CursoOfertas table
 migrationBuilder.CreateTable(
 name: "CursoOfertas",
 columns: table => new
 {
 Id = table.Column<int>(type: "int", nullable: false)
 .Annotation("SqlServer:Identity", "1,1"),
 CursoId = table.Column<int>(type: "int", nullable: false),
 CuatrimestreId = table.Column<int>(type: "int", nullable: false),
 NombreGrupo = table.Column<string>(type: "nvarchar(50)", maxLength:50, nullable: true),
 Capacidad = table.Column<int>(type: "int", nullable: true),
 FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
 },
 constraints: table =>
 {
 table.PrimaryKey("PK_CursoOfertas", x => x.Id);
 table.ForeignKey(
 name: "FK_CursoOfertas_Cursos_CursoId",
 column: x => x.CursoId,
 principalTable: "Cursos",
 principalColumn: "Id",
 onDelete: ReferentialAction.Cascade);
 table.ForeignKey(
 name: "FK_CursoOfertas_Cuatrimestres_CuatrimestreId",
 column: x => x.CuatrimestreId,
 principalTable: "Cuatrimestres",
 principalColumn: "Id",
 onDelete: ReferentialAction.Cascade);
 });

 // Create CursoOfertaDocentes table
 migrationBuilder.CreateTable(
 name: "CursoOfertaDocentes",
 columns: table => new
 {
 CursoOfertaId = table.Column<int>(type: "int", nullable: false),
 DocenteId = table.Column<int>(type: "int", nullable: false)
 },
 constraints: table =>
 {
 table.PrimaryKey("PK_CursoOfertaDocentes", x => new { x.CursoOfertaId, x.DocenteId });
 table.ForeignKey(
 name: "FK_CursoOfertaDocentes_CursoOfertas_CursoOfertaId",
 column: x => x.CursoOfertaId,
 principalTable: "CursoOfertas",
 principalColumn: "Id",
 onDelete: ReferentialAction.Cascade);
 table.ForeignKey(
 name: "FK_CursoOfertaDocentes_Usuarios_DocenteId",
 column: x => x.DocenteId,
 principalTable: "Usuarios",
 principalColumn: "Id",
 onDelete: ReferentialAction.Restrict);
 });

 // Add CursoOfertaId column to Matriculas (nullable, back-compat)
 migrationBuilder.AddColumn<int>(
 name: "CursoOfertaId",
 table: "Matriculas",
 type: "int",
 nullable: true);

 migrationBuilder.CreateIndex(
 name: "IX_CursoOfertas_CursoId",
 table: "CursoOfertas",
 column: "CursoId");

 migrationBuilder.CreateIndex(
 name: "IX_CursoOfertas_CuatrimestreId",
 table: "CursoOfertas",
 column: "CuatrimestreId");

 migrationBuilder.CreateIndex(
 name: "IX_CursoOfertaDocentes_DocenteId",
 table: "CursoOfertaDocentes",
 column: "DocenteId");

 migrationBuilder.CreateIndex(
 name: "IX_Matriculas_CursoOfertaId",
 table: "Matriculas",
 column: "CursoOfertaId");

 migrationBuilder.AddForeignKey(
 name: "FK_Matriculas_CursoOfertas_CursoOfertaId",
 table: "Matriculas",
 column: "CursoOfertaId",
 principalTable: "CursoOfertas",
 principalColumn: "Id",
 onDelete: ReferentialAction.Restrict);
 }

 protected override void Down(MigrationBuilder migrationBuilder)
 {
 migrationBuilder.DropForeignKey(
 name: "FK_Matriculas_CursoOfertas_CursoOfertaId",
 table: "Matriculas");

 migrationBuilder.DropTable(
 name: "CursoOfertaDocentes");

 migrationBuilder.DropTable(
 name: "CursoOfertas");

 migrationBuilder.DropIndex(
 name: "IX_Matriculas_CursoOfertaId",
 table: "Matriculas");

 migrationBuilder.DropColumn(
 name: "CursoOfertaId",
 table: "Matriculas");
 }
 }
}
