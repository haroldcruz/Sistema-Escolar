using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.DTOs.Evaluaciones
{
 public class EvaluacionCreateDTO
 {
 [Required]
 public int MatriculaId { get; set; }
 [Required]
 [Range(0,100)]
 public decimal Nota { get; set; }
 [MaxLength(2000)]
 public string Observaciones { get; set; } = string.Empty;
 [Required]
 [MaxLength(50)]
 public string Participacion { get; set; } = string.Empty;
 [Required]
 [MaxLength(50)]
 public string Estado { get; set; } = string.Empty; // e.g., "Aprobado" / "Reprobado"
 }
}
