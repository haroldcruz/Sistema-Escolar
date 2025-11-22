using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SistemaEscolar.Models;
using System.Linq;

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
 new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
 new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
 new Claim(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
 new Claim("nombreCompleto", $"{usuario.Nombre} {usuario.Apellidos}")
 };

 var permisosUnicos = new HashSet<string>();
 if (usuario.UsuarioRoles != null)
 {
 foreach (var ur in usuario.UsuarioRoles)
 {
 if (ur.Rol != null)
 {
 claims.Add(new Claim(ClaimTypes.Role, ur.Rol.Nombre));
 if (ur.Rol.RolPermisos != null)
 {
 foreach (var rp in ur.Rol.RolPermisos)
 {
 if (rp.Permiso != null && permisosUnicos.Add(rp.Permiso.Codigo))
 {
 claims.Add(new Claim("permiso", rp.Permiso.Codigo));
 }
 }
 }
 }
 }
 }

 var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
 var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

 var token = new JwtSecurityToken(
 issuer: _settings.Issuer,
 audience: _settings.Audience,
 claims: claims,
 expires: DateTime.UtcNow.AddMinutes(_settings.ExpireMinutes),
 signingCredentials: creds
 );

 return new JwtSecurityTokenHandler().WriteToken(token);
 }

 // Genera un refresh token aleatorio
 public string GenerateRefreshToken()
 {
 var randomNumber = new byte[32];
 using (var rng = RandomNumberGenerator.Create())
 {
 rng.GetBytes(randomNumber);
 return Convert.ToBase64String(randomNumber);
 }
 }
 }
}
