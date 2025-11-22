namespace SistemaEscolar.Models.Academico
{
 // Asignación de docentes a cursos (N:N)
 public class CursoDocente
 {
 public int CursoId { get; set; }
 public Curso Curso { get; set; } = null!;

 public int DocenteId { get; set; }
 public Usuario Docente { get; set; } = null!;

 public bool Activo { get; set; }
 }
}
