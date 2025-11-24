using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SistemaEscolar.DTOs.Bloques
{
 public class CalificacionItemDTO { public int MatriculaId { get; set; } [Range(0,100)] public decimal Nota { get; set; } public string Observaciones { get; set; } = string.Empty; public string Estado { get; set; } = string.Empty; public List<int>? Asistencias { get; set; } }
 public class CalificacionCreateDTO
 {
 [Required]
 public int BloqueId { get; set; }
 [Required]
 public List<CalificacionItemDTO> Items { get; set; } = new();
 }
}
