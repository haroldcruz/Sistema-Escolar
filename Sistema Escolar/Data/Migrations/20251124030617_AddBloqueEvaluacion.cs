using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema_Escolar.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBloqueEvaluacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "Usuarios",
                newName: "IsActive");

            migrationBuilder.CreateTable(
                name: "BloqueEvaluaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    CuatrimestreId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Peso = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloqueEvaluaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloqueEvaluaciones_Cuatrimestres_CuatrimestreId",
                        column: x => x.CuatrimestreId,
                        principalTable: "Cuatrimestres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BloqueEvaluaciones_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalificacionBloques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BloqueEvaluacionId = table.Column<int>(type: "int", nullable: false),
                    MatriculaId = table.Column<int>(type: "int", nullable: false),
                    Nota = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioRegistro = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionBloques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalificacionBloques_BloqueEvaluaciones_BloqueEvaluacionId",
                        column: x => x.BloqueEvaluacionId,
                        principalTable: "BloqueEvaluaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionBloques_Matriculas_MatriculaId",
                        column: x => x.MatriculaId,
                        principalTable: "Matriculas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloqueEvaluaciones_CuatrimestreId",
                table: "BloqueEvaluaciones",
                column: "CuatrimestreId");

            migrationBuilder.CreateIndex(
                name: "IX_BloqueEvaluaciones_CursoId",
                table: "BloqueEvaluaciones",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionBloques_BloqueEvaluacionId",
                table: "CalificacionBloques",
                column: "BloqueEvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionBloques_MatriculaId",
                table: "CalificacionBloques",
                column: "MatriculaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalificacionBloques");

            migrationBuilder.DropTable(
                name: "BloqueEvaluaciones");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Usuarios",
                newName: "Activo");

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Usuarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);
        }
    }
}
