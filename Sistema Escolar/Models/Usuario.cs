using System;
using System.Collections.Generic;
using SistemaEscolar.Models.Bitacora;

namespace SistemaEscolar.Models
{
 // Modelo de usuario del sistema
 public class Usuario
 {
 public int Id { get; set; } // PK
 public required string Nombre { get; set; }
 public required string Apellidos { get; set; }
 public required string Identificacion { get; set; }
 public required string Email { get; set; }

 public byte[] PasswordHash { get; set; } = Array.Empty<byte>(); // Hash
 public byte[] PasswordSalt { get; set; } = Array.Empty<byte>(); // Salt

 public bool Activo { get; set; }
 public DateTime FechaCreacion { get; set; }

 // Relación con roles
 public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();

 // Relación con bitácora
 public ICollection<BitacoraEntry> Bitacoras { get; set; } = new List<BitacoraEntry>();
 }
}
