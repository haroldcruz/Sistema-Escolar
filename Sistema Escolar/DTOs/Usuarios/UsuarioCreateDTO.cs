using System.Collections.Generic;

namespace SistemaEscolar.DTOs.Usuarios
{
 // Datos para crear un usuario
 public class UsuarioCreateDTO
 {
 public string Nombre { get; set; } = string.Empty;
 public string Apellidos { get; set; } = string.Empty;
 public string Identificacion { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;

 public string Password { get; set; } = string.Empty;

 public List<int> RolesIds { get; set; } = new(); // Asignación de roles
 }
}
