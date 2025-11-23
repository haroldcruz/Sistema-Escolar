using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

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

 // Create hash and salt for a plain password
 public static void CreatePasswordHash(string password, out string hashBase64, out string saltBase64)
 {
 if (password == null) throw new ArgumentNullException(nameof(password));

 // generate a128-bit salt using a secure PRNG
 byte[] salt = RandomNumberGenerator.GetBytes(128 /8);

 // derive a256-bit subkey (use HMACSHA256 with100,000 iterations)
 var derived = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256,100_000,32);

 hashBase64 = Convert.ToBase64String(derived);
 saltBase64 = Convert.ToBase64String(salt);
 }

 // Verify a password against stored hash+salt
 public static bool VerifyPassword(string password, string storedHashBase64, string storedSaltBase64)
 {
 if (password == null) return false;
 if (string.IsNullOrEmpty(storedHashBase64) || string.IsNullOrEmpty(storedSaltBase64)) return false;

 try
 {
 var salt = Convert.FromBase64String(storedSaltBase64);
 var stored = Convert.FromBase64String(storedHashBase64);
 var derived = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256,100_000,32);
 return CryptographicOperations.FixedTimeEquals(derived, stored);
 }
 catch
 {
 return false;
 }
 }
 }
}
