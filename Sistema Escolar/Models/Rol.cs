using System.Collections.Generic;

namespace SistemaEscolar.Models
{
 // Rol del sistema (Administrador, Docente, etc.)
 public class Rol
 {
 public int Id { get; set; } // PK
 public string Nombre { get; set; } = string.Empty; // Nombre único lógico

 // Relación N:N con Usuario a través de UsuarioRol
 public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();

 // Relación N:N con Permiso a través de RolPermiso
 public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
 }
}
