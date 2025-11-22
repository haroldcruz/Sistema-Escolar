using System;

namespace SistemaEscolar.Models.Auth
{
 // Token de refresco para JWT
 public class RefreshToken
 {
 public int Id { get; set; } // PK

 public required string Token { get; set; }
 public DateTime Expiracion { get; set; }
 public bool Revocado { get; set; }

 public int UsuarioId { get; set; } // FK
 public Usuario Usuario { get; set; } = null!;
 }
}
