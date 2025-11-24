using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.Models.Academico
{
 public class BloqueEvaluacion
 {
 public int Id { get; set; }

 [Required]
 public int CursoId { get; set; }
 public SistemaEscolar.Models.Academico.Curso? Curso { get; set; }

 [Required]
 public int CuatrimestreId { get; set; }
 public Cuatrimestre? Cuatrimestre { get; set; }

 [Required]
 [MaxLength(200)]
 public string Nombre { get; set; } = string.Empty;

 [MaxLength(100)]
 public string Tipo { get; set; } = "General"; // e.g., Tarea, Parcial, Asistencia

 public decimal? Peso { get; set; } // opcional

 public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
 public int? CreadoPorId { get; set; }

 public List<CalificacionBloque> Calificaciones { get; set; } = new();
 }
}
