using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.Models.Academico
{
 // Periodo académico
 public class Cuatrimestre
 {
 public int Id { get; set; }
 [Required]
 [MaxLength(100)]
 public string Nombre { get; set; } = string.Empty;
 // Número lógico para filtros (1..N)
 public int? Numero { get; set; }

 public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
 public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
 }
}
