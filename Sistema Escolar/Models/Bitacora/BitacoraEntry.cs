using System;
using SistemaEscolar.Models;

namespace SistemaEscolar.Models.Bitacora
{
 // Entradas de auditoría del sistema
 public class BitacoraEntry
 {
 public int Id { get; set; } // PK

 public int UsuarioId { get; set; } // FK
 public Usuario? Usuario { get; set; }

 public required string Accion { get; set; }
 public required string Modulo { get; set; }

 public required string Ip { get; set; }
 public DateTime Fecha { get; set; } = DateTime.UtcNow;
 }
}
