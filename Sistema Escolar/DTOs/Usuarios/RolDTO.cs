using System.Collections.Generic;

namespace SistemaEscolar.DTOs.Usuarios
{
 // Transferencia de datos del rol
 public class RolDTO
 {
 public int Id { get; set; }
 public string Nombre { get; set; } = string.Empty;
 public List<string> Permisos { get; set; } = new();
 }
}
