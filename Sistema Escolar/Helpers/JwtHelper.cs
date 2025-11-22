using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SistemaEscolar.Models;
using SistemaEscolar.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SistemaEscolar.Helpers
{
    // Genera JWT y refresh tokens
    public static class JwtHelper
    {
        // Genera el JWT principal con roles y permisos
        public static string GenerateToken(Usuario usuario, IEnumerable<string> roles, ApplicationDbContext ctx, JwtSettings settings)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(settings.Secret);

            // Permisos agregados por roles
            var permisos = ctx.RolPermisos.Include(rp => rp.Permiso)
                .Where(rp => rp.Rol.UsuarioRoles.Any(ur => ur.UsuarioId == usuario.Id))
                .Select(rp => rp.Permiso.Codigo)
                .Distinct()
                .ToList();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre + " " + usuario.Apellidos)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            claims.AddRange(permisos.Select(p => new Claim("permiso", p)));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = settings.Issuer,
                Audience = settings.Audience
            };
            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }
    }
}
