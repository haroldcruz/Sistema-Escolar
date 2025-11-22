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
 public string? Cuatrimestre { get; set; }

 public List<string> Docentes { get; set; } = new();
 }
}
