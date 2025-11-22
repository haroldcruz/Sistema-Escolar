using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Auth;
using SistemaEscolar.Interfaces.Auth;
using SistemaEscolar.Models;
using SistemaEscolar.Models.Auth;
using SistemaEscolar.Helpers;
using System.Linq;
using SistemaEscolar.Interfaces.Bitacora;
using Microsoft.AspNetCore.Http;

namespace SistemaEscolar.Services.Auth
{
 // Implementación del servicio de autenticación
 public class AuthService : IAuthService
 {
 private readonly ApplicationDbContext _context;
 private readonly JwtHelper _jwt;
 private readonly IBitacoraService _bitacora;
 private readonly IHttpContextAccessor _http;

 public AuthService(ApplicationDbContext context, JwtHelper jwt, IBitacoraService bitacora, IHttpContextAccessor http)
 {
 _context = context;
 _jwt = jwt;
 _bitacora = bitacora;
 _http = http;
 }

 private string Ip() => _http.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";

 // Login
 public async Task<LoginResponse?> LoginAsync(LoginRequest request)
 {
 var usuario = await _context.Usuarios
 .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol).ThenInclude(r => r.RolPermisos).ThenInclude(rp => rp.Permiso)
 .AsSplitQuery()
 .FirstOrDefaultAsync(u => u.Email == request.Email);

 if (usuario == null)
 return null; // no revelar detalles

 // Validación de contraseña
 if (!PasswordHasher.VerifyPassword(request.Password, usuario.PasswordHash, usuario.PasswordSalt))
 return null;

 var token = await GenerateJwtTokenAsync(usuario.Id);
 var refreshToken = await GenerateRefreshTokenAsync(usuario.Id);

 // Bitácora login
 await _bitacora.RegistrarLoginAsync(usuario.Id, Ip());

 var rolPrincipal = usuario.UsuarioRoles.Select(ur => ur.Rol.Nombre).FirstOrDefault() ?? string.Empty;

 return new LoginResponse
 {
 Token = token,
 RefreshToken = refreshToken,
 UsuarioId = usuario.Id,
 NombreCompleto = $"{usuario.Nombre} {usuario.Apellidos}",
 Email = usuario.Email,
 RolPrincipal = rolPrincipal
 };
 }

 // Genera JWT
 public async Task<string> GenerateJwtTokenAsync(int usuarioId)
 {
 var usuario = await _context.Usuarios
 .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol).ThenInclude(r => r.RolPermisos).ThenInclude(rp => rp.Permiso)
 .AsSplitQuery()
 .FirstOrDefaultAsync(u => u.Id == usuarioId);

 return _jwt.GenerateToken(usuario!);
 }

 // Crea refresh token
 private async Task<string> GenerateRefreshTokenAsync(int usuarioId)
 {
 var token = _jwt.GenerateRefreshToken();

 var entity = new RefreshToken
 {
 UsuarioId = usuarioId,
 Token = token,
 Expiracion = DateTime.UtcNow.AddDays(7),
 Revocado = false
 };

 _context.RefreshTokens.Add(entity);
 await _context.SaveChangesAsync();
 return token;
 }

 public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request)
 {
 var stored = await _context.RefreshTokens
 .Include(r => r.Usuario).ThenInclude(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol).ThenInclude(r => r.RolPermisos).ThenInclude(rp => rp.Permiso)
 .AsSplitQuery()
 .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && !r.Revocado);

 if (stored == null || stored.Expiracion < DateTime.UtcNow)
 return null;

 var newJwt = await GenerateJwtTokenAsync(stored.UsuarioId);
 // Bitácora refresh
 await _bitacora.RegistrarAsync(stored.UsuarioId, "RefreshToken", "Autenticacion", Ip());

 var rolPrincipal = stored.Usuario.UsuarioRoles.Select(ur => ur.Rol.Nombre).FirstOrDefault() ?? string.Empty;

 return new LoginResponse
 {
 Token = newJwt,
 RefreshToken = stored.Token,
 UsuarioId = stored.Usuario.Id,
 NombreCompleto = $"{stored.Usuario.Nombre} {stored.Usuario.Apellidos}",
 Email = stored.Usuario.Email,
 RolPrincipal = rolPrincipal
 };
 }

 public async Task<bool> RevokeRefreshTokenAsync(int usuarioId)
 {
 var tokens = await _context.RefreshTokens
 .Where(t => t.UsuarioId == usuarioId && !t.Revocado)
 .ToListAsync();

 foreach (var token in tokens)
 token.Revocado = true;

 await _context.SaveChangesAsync();
 await _bitacora.RegistrarAsync(usuarioId, "RevokeTokens", "Autenticacion", Ip());
 return true;
 }
 }
}
