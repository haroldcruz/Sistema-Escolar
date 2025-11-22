namespace SistemaEscolar.DTOs.Bitacora
{
 // Representa una entrada de bitácora
 public class BitacoraDTO
 {
 public int Id { get; set; }
 public string Usuario { get; set; } = string.Empty;
 public string Accion { get; set; } = string.Empty;
 public string Modulo { get; set; } = string.Empty;
 public string Ip { get; set; } = string.Empty;
 public string Fecha { get; set; } = string.Empty;
 }
}
