using System;
using System.Security.Cryptography;
using System.Text;

namespace SistemaEscolar.Helpers
{
 // Simple PBKDF2-based password hasher compatible with byte[] storage
 public static class PasswordHasher
 {
 // Create hash and salt
 public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
 {
 if (password == null) throw new ArgumentNullException(nameof(password));
 using var hmac = new HMACSHA512();
 salt = hmac.Key;
 hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
 }

 // Verify provided password against stored hash and salt
 public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
 {
 if (password == null) throw new ArgumentNullException(nameof(password));
 if (storedHash == null || storedHash.Length ==0) return false;
 if (storedSalt == null || storedSalt.Length ==0) return false;

 using var hmac = new HMACSHA512(storedSalt);
 var computed = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
 if (computed.Length != storedHash.Length) return false;
 for (int i =0; i < computed.Length; i++) if (computed[i] != storedHash[i]) return false;
 return true;
 }
 }
}
