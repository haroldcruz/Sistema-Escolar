using System.Collections.Generic;

namespace SistemaEscolar.Models
{
 // Permiso granular (ej: Cursos.Crear, Cursos.Editar, Usuarios.Ver, Bitacora.Ver)
 public class Permiso
 {
 public int Id { get; set; } // PK
 public string Codigo { get; set; } = string.Empty; // único, usado en policies
 public string Descripcion { get; set; } = string.Empty;

 public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
 }
}