using System.Collections.Generic;

namespace SistemaEscolar.DTOs.Historial
{
 // Historial completo del estudiante
 public class EstudianteHistorialDTO
 {
 public int EstudianteId { get; set; }
 public string NombreCompleto { get; set; } = string.Empty;

 public List<HistorialItemDTO> Registros { get; set; } = new();
 }
}
