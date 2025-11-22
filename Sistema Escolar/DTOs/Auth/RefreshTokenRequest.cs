namespace SistemaEscolar.DTOs.Auth
{
 // Solicitud para renovar el JWT
 public class RefreshTokenRequest
 {
 public required string Token { get; set; }
 public required string RefreshToken { get; set; }
 }
}
