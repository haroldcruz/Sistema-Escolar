using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.DTOs.Usuarios
{
 // Datos para crear un usuario
 public class UsuarioCreateDTO
 {
 [Required]
 [MaxLength(100)]
 public string Nombre { get; set; } = string.Empty;
 [MaxLength(200)]
 public string Apellidos { get; set; } = string.Empty;
 [Required]
 [EmailAddress]
 public string Email { get; set; } = string.Empty;
 [Required]
 [MaxLength(50)]
 public string Identificacion { get; set; } = string.Empty;
 [Required]
 [MinLength(6)]
 public string Password { get; set; } = string.Empty;

 // Roles seleccionados
 public List<int> RolesIds { get; set; } = new();
 }
}
