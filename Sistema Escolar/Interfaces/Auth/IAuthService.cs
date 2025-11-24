using System.Threading.Tasks;
using SistemaEscolar.DTOs.Auth;

namespace SistemaEscolar.Interfaces.Auth
{
 public interface IAuthService
 {
 Task<LoginResponse?> AuthenticateAsync(string email, string password);
 Task<LoginResponse?> LoginAsync(LoginRequest request);
 Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request);
 Task<bool> RevokeRefreshTokenAsync(int usuarioId);
 }
}
