using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.DTOs.Cursos
{
 // Datos necesarios para crear un curso
 public class CursoCreateDTO
 {
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

 [Required]
 public int CuatrimestreId { get; set; } // ahora obligatorio
 }
}
