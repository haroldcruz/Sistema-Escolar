using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema_Escolar.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CursoDocentes_Cursos_CursoId",
                table: "CursoDocentes");

            migrationBuilder.DropForeignKey(
                name: "FK_CursoDocentes_Usuarios_DocenteId",
                table: "CursoDocentes");

            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Cuatrimestres_CuatrimestreId",
                table: "Cursos");

            migrationBuilder.DropForeignKey(
                name: "FK_Matriculas_Cuatrimestres_CuatrimestreId",
                table: "Matriculas");

            migrationBuilder.DropForeignKey(
                name: "FK_Matriculas_Cursos_CursoId",
                table: "Matriculas");

            migrationBuilder.AlterColumn<int>(
                name: "EstudianteId",
                table: "Matriculas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CursoId",
                table: "Matriculas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CuatrimestreId",
                table: "Matriculas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CuatrimestreId",
                table: "Cursos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CursoDocentes_Cursos_CursoId",
                table: "CursoDocentes",
                column: "CursoId",
                principalTable: "Cursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CursoDocentes_Usuarios_DocenteId",
                table: "CursoDocentes",
                column: "DocenteId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Cuatrimestres_CuatrimestreId",
                table: "Cursos",
                column: "CuatrimestreId",
                principalTable: "Cuatrimestres",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Matriculas_Cuatrimestres_CuatrimestreId",
                table: "Matriculas",
                column: "CuatrimestreId",
                principalTable: "Cuatrimestres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matriculas_Cursos_CursoId",
                table: "Matriculas",
                column: "CursoId",
                principalTable: "Cursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CursoDocentes_Cursos_CursoId",
                table: "CursoDocentes");

            migrationBuilder.DropForeignKey(
                name: "FK_CursoDocentes_Usuarios_DocenteId",
                table: "CursoDocentes");

            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Cuatrimestres_CuatrimestreId",
                table: "Cursos");

            migrationBuilder.DropForeignKey(
                name: "FK_Matriculas_Cuatrimestres_CuatrimestreId",
                table: "Matriculas");

            migrationBuilder.DropForeignKey(
                name: "FK_Matriculas_Cursos_CursoId",
                table: "Matriculas");

            migrationBuilder.AlterColumn<int>(
                name: "EstudianteId",
                table: "Matriculas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CursoId",
                table: "Matriculas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CuatrimestreId",
                table: "Matriculas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CuatrimestreId",
                table: "Cursos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CursoDocentes_Cursos_CursoId",
                table: "CursoDocentes",
                column: "CursoId",
                principalTable: "Cursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CursoDocentes_Usuarios_DocenteId",
                table: "CursoDocentes",
                column: "DocenteId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Cuatrimestres_CuatrimestreId",
                table: "Cursos",
                column: "CuatrimestreId",
                principalTable: "Cuatrimestres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Matriculas_Cuatrimestres_CuatrimestreId",
                table: "Matriculas",
                column: "CuatrimestreId",
                principalTable: "Cuatrimestres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Matriculas_Cursos_CursoId",
                table: "Matriculas",
                column: "CursoId",
                principalTable: "Cursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
