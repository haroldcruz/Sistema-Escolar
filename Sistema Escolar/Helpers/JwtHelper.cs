using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SistemaEscolar.Models;

namespace SistemaEscolar.Helpers
{
    // Genera JWT y refresh tokens
    public class JwtHelper
    {
        private readonly JwtSettings _settings;

        public JwtHelper(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        // Genera el JWT principal con roles y permisos
        public string GenerateToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellidos}"),
                new Claim(ClaimTypes.Email, usuario.Email ?? string.Empty)
            };

            // Roles
            var roles = usuario.UsuarioRoles?.Select(ur => ur.Rol?.Nombre).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct() ?? Enumerable.Empty<string>();
            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r!));

            // Permisos (claim personalizado "permiso")
            var permisos = usuario.UsuarioRoles?
                .Where(ur => ur.Rol != null && ur.Rol.RolPermisos != null)
                .SelectMany(ur => ur.Rol!.RolPermisos!)
                .Where(rp => rp.Permiso != null)
                .Select(rp => rp.Permiso!.Codigo)
                .Distinct() ?? Enumerable.Empty<string>();
            foreach (var p in permisos)
                claims.Add(new Claim("permiso", p));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes > 0 ? _settings.ExpirationMinutes : 60),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Genera un refresh token aleatorio
        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
