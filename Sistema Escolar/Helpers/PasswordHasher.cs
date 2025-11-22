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
 using (var hmac = new HMACSHA512())
 {
 salt = hmac.Key;
 hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
 }
 }

 // Verifica contraseña
 public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
 {
 if (storedHash == null || storedSalt == null)
 return false;

 using (var hmac = new HMACSHA512(storedSalt))
 {
 var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
 if (computedHash.Length != storedHash.Length)
 return false;

 for (int i =0; i < computedHash.Length; i++)
 {
 if (computedHash[i] != storedHash[i])
 return false;
 }
 }

 return true;
 }
 }
}
