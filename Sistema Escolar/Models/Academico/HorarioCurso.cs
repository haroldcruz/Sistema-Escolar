using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.Models.Academico
{
 public class HorarioCurso
 {
 public int Id { get; set; }

 [Required]
 public int CursoId { get; set; }
 public Curso Curso { get; set; } = null!;

 //0=Sunday,1=Monday ...6=Saturday
 [Range(0,6)]
 public int DiaSemana { get; set; }

 [Required]
 public TimeSpan HoraInicio { get; set; }

 [Required]
 public TimeSpan HoraFin { get; set; }
 }
}
