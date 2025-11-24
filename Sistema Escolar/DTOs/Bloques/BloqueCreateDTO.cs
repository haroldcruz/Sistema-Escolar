using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.DTOs.Bloques
{
 public class BloqueCreateDTO
 {
 [Required]
 public int CursoId { get; set; }
 [Required]
 public int CuatrimestreId { get; set; }
 [Required]
 [MaxLength(200)]
 public string Nombre { get; set; } = string.Empty;
 [MaxLength(100)]
 public string Tipo { get; set; } = "General";
 public decimal? Peso { get; set; }
 public DateTime? FechaAsignacion { get; set; }
 public List<DateTime>? FechasAsistencia { get; set; }
 }
}
