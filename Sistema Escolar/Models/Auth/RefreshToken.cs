using System;

namespace SistemaEscolar.Models.Auth
{
 // Token de refresco para JWT
 public class RefreshToken
 {
 public int Id { get; set; } // PK

 public required string Token { get; set; } = string.Empty;
 public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
 // Nombre de columna según migración inicial: Expiracion
 public DateTime Expiracion { get; set; }
 // Nombre de columna según migración inicial: Revocado
 public bool Revocado { get; set; }

 public int UsuarioId { get; set; } // FK
 public Usuario Usuario { get; set; } = null!;
 }
}
