using System.Collections.Generic;

namespace SistemaEscolar.DTOs.Usuarios
{
 // Datos generales del usuario
 public class UsuarioDTO
 {
 public int Id { get; set; }
 public string NombreCompleto { get; set; } = string.Empty;
 public string Identificacion { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;

 public List<string> Roles { get; set; } = new(); // Roles asignados
 }
}
