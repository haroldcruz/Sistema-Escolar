using System.Collections.Generic;

namespace SistemaEscolar.DTOs.Historial
{
 // Grupo de items por cuatrimestre
 public class HistorialCuatrimestreDTO
 {
 public string Cuatrimestre { get; set; } = string.Empty;
 public List<HistorialItemDTO> Cursos { get; set; } = new();
 public decimal? Promedio { get; set; }
 }
}