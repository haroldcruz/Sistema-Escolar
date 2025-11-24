using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.DTOs.Cursos
{
 public class NotaFinalItemDTO { public int MatriculaId { get; set; } public decimal NotaFinal { get; set; } }
 public class NotaFinalCreateDTO
 {
 public int? CuatrimestreId { get; set; }
 [Required]
 public List<NotaFinalItemDTO> Items { get; set; } = new();
 }
}
