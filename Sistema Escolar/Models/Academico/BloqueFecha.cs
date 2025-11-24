using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.Models.Academico
{
 public class BloqueFecha
 {
 public int Id { get; set; }

 [Required]
 public int BloqueEvaluacionId { get; set; }
 public BloqueEvaluacion? BloqueEvaluacion { get; set; }

 [Required]
 public DateTime Fecha { get; set; }
 }
}