using System.Threading.Tasks;
using SistemaEscolar.DTOs.Auth;
using SistemaEscolar.Models;
using SistemaEscolar.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Helpers;
using SistemaEscolar.Models.Auth;
using System;
using SistemaEscolar.Interfaces.Auth;
using SistemaEscolar.Interfaces.Bitacora;

namespace SistemaEscolar.Services.Auth
{
    // Implementación del servicio de autenticación
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _ctx;
        private readonly JwtSettings _settings;
        private readonly IBitacoraService _bitacora;

        public AuthService(ApplicationDbContext ctx, JwtSettings settings, IBitacoraService bitacora)
        {
            _ctx = ctx;
            _settings = settings;
            _bitacora = bitacora;
        }

        // Login
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _ctx.Usuarios
                .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !user.Activo) return null;

            // Validación de contraseña
            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt)) return null;

            var roles = user.UsuarioRoles.Select(r => r.Rol.Nombre).ToList();
            var token = JwtHelper.GenerateToken(user, roles, _ctx, _settings);

            // Bitácora login
            await _bitacora.RegistrarLoginAsync(user.Id, "0.0.0.0");

            // Refresh token
            var refresh = new RefreshToken { UsuarioId = user.Id, Token = Guid.NewGuid().ToString("N"), Expiracion = DateTime.UtcNow.AddDays(7), Revocado = false };
            _ctx.RefreshTokens.Add(refresh);
            await _ctx.SaveChangesAsync();

            return new LoginResponse { Token = token, RefreshToken = refresh.Token, Roles = roles, Email = user.Email, NombreCompleto = user.Nombre + " " + user.Apellidos, RolPrincipal = roles.FirstOrDefault() ?? string.Empty, UsuarioId = user.Id };
        }

        public async Task<string> GenerateJwtTokenAsync(int usuarioId)
        {
            var user = await _ctx.Usuarios
                .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (user == null) return string.Empty;

            var roles = user.UsuarioRoles.Select(r => r.Rol.Nombre).ToList();
            return JwtHelper.GenerateToken(user, roles, _ctx, _settings);
        }

        public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var rt = await _ctx.RefreshTokens
                .Include(r => r.Usuario).ThenInclude(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

            if (rt == null || rt.Revocado || rt.Expiracion < DateTime.UtcNow) return null;

            var user = rt.Usuario;
            var roles = user.UsuarioRoles.Select(r => r.Rol.Nombre).ToList();
            var newJwt = JwtHelper.GenerateToken(user, roles, _ctx, _settings);

            return new LoginResponse { Token = newJwt, RefreshToken = rt.Token, Roles = roles, Email = user.Email, NombreCompleto = user.Nombre + " " + user.Apellidos, RolPrincipal = roles.FirstOrDefault() ?? string.Empty, UsuarioId = user.Id };
        }

        public async Task<bool> RevokeRefreshTokenAsync(int usuarioId)
        {
            var tokens = _ctx.RefreshTokens.Where(r => r.UsuarioId == usuarioId && !r.Revocado).ToList();

            if (!tokens.Any()) return false;

            foreach (var t in tokens) { t.Revocado = true; }

            await _ctx.SaveChangesAsync();
            return true;
        }
    }
}
