namespace SistemaEscolar.Models
{
 // Tabla puente N:N entre Rol y Permiso
 public class RolPermiso
 {
 public int RolId { get; set; }
 public Rol Rol { get; set; } = null!;

 public int PermisoId { get; set; }
 public Permiso Permiso { get; set; } = null!;
 }
}