namespace SistemaEscolar.DTOs.Usuarios
{
 public class RolCreateDTO
 {
 public string Nombre { get; set; } = string.Empty;
 public List<int> PermisosIds { get; set; } = new();
 }
 public class RolUpdateDTO
 {
 public int Id { get; set; }
 public string Nombre { get; set; } = string.Empty;
 public List<int> PermisosIds { get; set; } = new();
 }
}
