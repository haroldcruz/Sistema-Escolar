using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.Models.Academico
{
 public class CalificacionBloque
 {
 public int Id { get; set; }
 public int BloqueEvaluacionId { get; set; }
 public BloqueEvaluacion? BloqueEvaluacion { get; set; }

 public int MatriculaId { get; set; }
 public SistemaEscolar.Models.Academico.Matricula? Matricula { get; set; }

 [Range(0,100)]
 public decimal Nota { get; set; }

 [MaxLength(2000)]
 public string Observaciones { get; set; } = string.Empty;

 [MaxLength(50)]
 public string Estado { get; set; } = string.Empty; // Aprobado/Reprobado optional

 public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
 public int UsuarioRegistro { get; set; }
 }
}
