namespace SistemaEscolar.DTOs.Auth
{
 // Datos enviados al hacer login
 public class LoginRequest
 {
 public string Email { get; set; } = string.Empty;
 public string Password { get; set; } = string.Empty;
 }
}
