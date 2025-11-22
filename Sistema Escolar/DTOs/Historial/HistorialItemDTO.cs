namespace SistemaEscolar.DTOs.Historial
{
 // Item individual del historial académico
 public class HistorialItemDTO
 {
 public string Curso { get; set; } = string.Empty;
 public string Cuatrimestre { get; set; } = string.Empty;
 public decimal Nota { get; set; }
 public string Estado { get; set; } = string.Empty;
 public string Participacion { get; set; } = string.Empty;
 public string Observaciones { get; set; } = string.Empty;

 public string Fecha { get; set; } = string.Empty; // texto corto
 }
}
