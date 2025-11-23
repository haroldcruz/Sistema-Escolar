using System.Collections.Generic;

namespace SistemaEscolar.DTOs.Cursos
{
 // Representación de curso para vistas y API
 public class CursoDTO
 {
 public int Id { get; set; }
 public string Codigo { get; set; } = string.Empty;
 public string Nombre { get; set; } = string.Empty;
 public string Descripcion { get; set; } = string.Empty;
 public int Creditos { get; set; }
 public int? CuatrimestreId { get; set; }
 // Nombre legible del cuatrimestre (ej. "I", "II", "2025-1", etc.)
 public string? Cuatrimestre { get; set; }
 // Nuevo: número de cuatrimestre para filtros lógicos en UI
 public int? CuatrimestreNumero { get; set; }

 public List<string> Docentes { get; set; } = new();
 }
}
