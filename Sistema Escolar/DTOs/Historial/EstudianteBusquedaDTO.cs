namespace SistemaEscolar.DTOs.Historial
{
 // Resultado liviano para autocompletar/búsqueda
 public class EstudianteBusquedaDTO
 {
 public int Id { get; set; }
 public string NombreCompleto { get; set; } = string.Empty;
 public string Identificacion { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;
 }
}