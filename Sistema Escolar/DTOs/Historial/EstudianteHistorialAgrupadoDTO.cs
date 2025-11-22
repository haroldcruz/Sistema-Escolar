using System.Collections.Generic;

namespace SistemaEscolar.DTOs.Historial
{
 // Historial agrupado por cuatrimestre con métricas
 public class EstudianteHistorialAgrupadoDTO
 {
 public int EstudianteId { get; set; }
 public string NombreCompleto { get; set; } = string.Empty;
 public List<HistorialCuatrimestreDTO> Cuatrimestres { get; set; } = new();
 public decimal? PromedioGeneral { get; set; }
 }
}