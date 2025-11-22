namespace SistemaEscolar.Helpers
{
 // Configuración de JWT leída desde appsettings
 public class JwtSettings
 {
 public string Secret { get; set; } = string.Empty; // Clave secreta
 public string Issuer { get; set; } = string.Empty; // Emisor
 public string Audience { get; set; } = string.Empty; // Audiencia
 public int ExpirationMinutes { get; set; } =60; // Minutos de vigencia
 }
}
