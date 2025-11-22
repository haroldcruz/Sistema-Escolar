using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.DTOs.Usuarios
{
 // Datos para crear un usuario
 public class UsuarioCreateDTO
 {
 public string Nombre { get; set; } = string.Empty;
 public string Apellidos { get; set; } = string.Empty;
 public string Identificacion { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;

 [Required]
 [MinLength(6)]
 public string Password { get; set; } = string.Empty;

 [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
 public string ConfirmPassword { get; set; } = string.Empty;

 public List<int> RolesIds { get; set; } = new(); // Asignación de roles
 }
}
