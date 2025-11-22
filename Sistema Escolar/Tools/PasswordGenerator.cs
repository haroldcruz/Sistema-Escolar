using System.Security.Cryptography;
using System.Text;

namespace SistemaEscolar.Tools
{
 // Generador de contraseñas seguras
 public static class PasswordGenerator
 {
 private const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
 private const string Digits = "0123456789";
 private const string Symbols = "!@#$%^&*()-_=+[]{};:,.<>?";

 public static string Generate(int length =16, bool includeSymbols = true)
 {
 if (length <8) length =8; // mínimo razonable

 var charset = Letters + Digits + (includeSymbols ? Symbols : string.Empty);
 var bytes = new byte[length];
 using var rng = RandomNumberGenerator.Create();
 rng.GetBytes(bytes);

 var sb = new StringBuilder(length);
 for (int i =0; i < length; i++)
 {
 sb.Append(charset[bytes[i] % charset.Length]);
 }

 // Asegurar al menos1 dígito y1 símbolo si se pidió incluir símbolos
 if (!sb.ToString().Any(char.IsDigit))
 {
 sb[0] = Digits[bytes[0] % Digits.Length];
 }
 if (includeSymbols && !sb.ToString().Any(c => Symbols.Contains(c)))
 {
 sb[1] = Symbols[bytes[1] % Symbols.Length];
 }

 return sb.ToString();
 }
 }
}
