using System;
using System.Collections.Generic;
using SistemaEscolar.Models.Bitacora;

namespace SistemaEscolar.Models
{
 // Modelo de usuario
 public class Usuario
 {
 public int Id { get; set; }
 public string Nombre { get; set; } = string.Empty;
 public string Apellidos { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;
 public string Identificacion { get; set; } = string.Empty;
 public byte[] PasswordHash { get; set; } = System.Array.Empty<byte>();
 public byte[] PasswordSalt { get; set; } = System.Array.Empty<byte>();
 public bool IsActive { get; set; } = true;

 // Relaciones
 public List<UsuarioRol> UsuarioRoles { get; set; } = new();
 public List<Bitacora.BitacoraEntry> Bitacoras { get; set; } = new();
 }
}
