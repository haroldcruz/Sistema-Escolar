namespace SistemaEscolar.Models
{
 // Relación N:N entre Usuarios y Roles
 public class UsuarioRol
 {
 public int UsuarioId { get; set; }
 public Usuario Usuario { get; set; } = null!;

 public int RolId { get; set; }
 public Rol Rol { get; set; } = null!;
 }
}
