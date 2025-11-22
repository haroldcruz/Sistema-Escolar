namespace SistemaEscolar.DTOs.Cursos
{
 // Datos necesarios para crear un curso
 public class CursoCreateDTO
 {
 public string Codigo { get; set; } = string.Empty;
 public string Nombre { get; set; } = string.Empty;
 public string Descripcion { get; set; } = string.Empty;
 public int Creditos { get; set; }

 public int? CuatrimestreId { get; set; } // ahora opcional
 }
}
