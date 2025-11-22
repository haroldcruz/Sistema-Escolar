namespace SistemaEscolar.Helpers
{
 // Configuración de JWT leída desde appsettings
 public class JwtSettings
 {
 public string Key { get; set; } = string.Empty; // Clave secreta
 public string Issuer { get; set; } = string.Empty; // Emisor
 public string Audience { get; set; } = string.Empty; // Audiencia
 public int ExpireMinutes { get; set; } // Minutos de vigencia
 }
}
