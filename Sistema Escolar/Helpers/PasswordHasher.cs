using System;
using System.Security.Cryptography;
using System.Text;

namespace SistemaEscolar.Helpers
{
 // Maneja hash y verificación de contraseñas
 public static class PasswordHasher
 {
 // Crea hash + salt
 public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
 {
 using var hmac = new HMACSHA512(); // volver a HMACSHA512 para coincidir con hashes previamente generados
 salt = hmac.Key;
 hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
 }

 // Verifica contraseña
 public static bool VerifyPassword(string password, byte[] hash, byte[] salt)
 {
 using var hmac = new HMACSHA512(salt);
 var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
 if (computed.Length != hash.Length) return false;
 for (int i=0;i<computed.Length;i++) if (computed[i]!=hash[i]) return false;
 return true;
 }
 }
}
