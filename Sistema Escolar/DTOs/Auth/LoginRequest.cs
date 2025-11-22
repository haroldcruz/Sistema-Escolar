namespace SistemaEscolar.DTOs.Auth
{
 // Datos enviados al hacer login
 public class LoginRequest
 {
 public required string Email { get; set; }
 public required string Password { get; set; }
 }
}
