namespace SistemaEscolar.Models.Security
{
 // Configuración para bloqueo por intentos fallidos (bind desde appsettings: Security:Lockout)
 public class LockoutSettings
 {
 public int MaxFailedAttempts { get; set; } =5;
 public int LockoutMinutes { get; set; } =15;
 public bool CaptchaEnabled { get; set; } = false;
 }
}