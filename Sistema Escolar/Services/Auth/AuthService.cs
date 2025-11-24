using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Auth;
using SistemaEscolar.Interfaces.Auth;
using SistemaEscolar.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SistemaEscolar.Services.Auth
{
    // Implementación del servicio de autenticación
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _ctx;
        private readonly SistemaEscolar.Models.Auth.JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;
        public AuthService(ApplicationDbContext ctx, SistemaEscolar.Models.Auth.JwtSettings jwtSettings, ILogger<AuthService> logger){ _ctx = ctx; _jwtSettings = jwtSettings; _logger = logger; }

        public async Task<LoginResponse?> AuthenticateAsync(string email, string password)
        {
            email = email?.Trim() ?? string.Empty;
            var user = await _ctx.Usuarios.Include(u=>u.UsuarioRoles).ThenInclude(ur=>ur.Rol).FirstOrDefaultAsync(u=>u.Email==email);
            if (user == null)
            {
                _logger.LogWarning("Auth: user not found for email {Email}", email);
                return null;
            }
            // verificar password
            try
            {
                _logger.LogInformation("Auth: user found Id={Id} Email={Email} PasswordHashLen={HashLen} PasswordSaltLen={SaltLen}", user.Id, user.Email, user.PasswordHash?.Length ??0, user.PasswordSalt?.Length ??0);
                bool verified = SistemaEscolar.Helpers.PasswordHasher.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt);
                _logger.LogInformation("Auth: password verify result for user {Email}: {Result}", user.Email, verified);
                if (!verified) return null;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Auth: error verifying password for {Email}", email);
                return null;
            }
            // Activo puede ser bool o int -> intentar manejar ambos
            // si existe propiedad Activo como bool
            var activoProp = user.GetType().GetProperty("Activo");
            if (activoProp != null)
            {
                var val = activoProp.GetValue(user);
                if (val is bool b && !b) return null;
                if (val is int i && i ==0) return null;
            }

            // roles
            var roles = user.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList();
            // generar token con claims (JwtHelper requiere ctx y settings)
            var token = SistemaEscolar.Helpers.JwtHelper.GenerateToken(user, roles, _ctx, _jwtSettings);
            // refresh token omitido
            var response = new LoginResponse { Token = token, RefreshToken = string.Empty, UsuarioId = user.Id, NombreCompleto = user.Nombre + " " + user.Apellidos, Email = user.Email, RolPrincipal = roles.FirstOrDefault() ?? string.Empty, Roles = roles };
            return response;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            return await AuthenticateAsync(request.Email, request.Password);
        }

        public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // Simplified: not implementing refresh token storage. Reject.
            return null;
        }

        public async Task<bool> RevokeRefreshTokenAsync(int usuarioId)
        {
            // Simplified: no refresh token storage
            return await Task.FromResult(true);
        }
    }
}
