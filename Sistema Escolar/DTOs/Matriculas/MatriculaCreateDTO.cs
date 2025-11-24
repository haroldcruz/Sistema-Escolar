using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.DTOs.Matriculas
{
 public class MatriculaCreateDTO
 {
 [Required]
 public int EstudianteId { get; set; }

 [Required]
 public int CuatrimestreId { get; set; }

 [Required]
 [MinLength(1, ErrorMessage = "Debe seleccionar al menos un curso")]
 public List<int> CursosIds { get; set; } = new();
 }
}
