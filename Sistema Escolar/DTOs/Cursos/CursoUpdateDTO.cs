using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.DTOs.Cursos
{
 // Datos para actualizar curso
 public class CursoUpdateDTO
 {
 public int Id { get; set; }

 [Required]
 [MaxLength(50)]
 public string Codigo { get; set; } = string.Empty;

 [Required]
 [MaxLength(200)]
 public string Nombre { get; set; } = string.Empty;

 [MaxLength(2000)]
 public string Descripcion { get; set; } = string.Empty;

 [Range(0,50)]
 public int Creditos { get; set; }

 public int? CuatrimestreId { get; set; }
 }
}
