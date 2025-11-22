using System.Collections.Generic;

namespace SistemaEscolar.Models.Academico
{
 // Periodo académico
 public class Cuatrimestre
 {
 public int Id { get; set; } // PK
 public required string Nombre { get; set; }
 public int Anio { get; set; }

 public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
 public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
 }
}
