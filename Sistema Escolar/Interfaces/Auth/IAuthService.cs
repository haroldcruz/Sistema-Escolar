using SistemaEscolar.DTOs.Auth;
using System.Threading.Tasks;

namespace SistemaEscolar.Interfaces.Auth
{
 // Define operaciones de autenticación
 public interface IAuthService
 {
 Task<LoginResponse?> LoginAsync(LoginRequest request); // login principal
 Task<string> GenerateJwtTokenAsync(int usuarioId); // genera el JWT
 Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request); // renueva
 Task<bool> RevokeRefreshTokenAsync(int usuarioId); // invalida token
 }
}
