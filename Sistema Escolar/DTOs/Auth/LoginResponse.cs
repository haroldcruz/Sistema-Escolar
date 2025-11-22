namespace SistemaEscolar.DTOs.Auth
{
 // Respuesta con tokens y datos del usuario
 public class LoginResponse
 {
 public required string Token { get; set; } // JWT
 public required string RefreshToken { get; set; }

 public int UsuarioId { get; set; }
 public required string NombreCompleto { get; set; }
 public required string Email { get; set; }
 public required string RolPrincipal { get; set; }
 }
}
