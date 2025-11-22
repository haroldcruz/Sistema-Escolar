using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.DTOs.Auth;
using SistemaEscolar.Interfaces.Auth;
using System.IdentityModel.Tokens.Jwt;

namespace SistemaEscolar.Controllers.API
{
 // Controlador API para autenticación
 [ApiController]
 [Route("api/[controller]")]
 public class AuthApiController : ControllerBase
 {
 private readonly IAuthService _auth;

 public AuthApiController(IAuthService auth)
 {
 _auth = auth;
 }

 // POST: api/auth/login
 [HttpPost("login")]
 [AllowAnonymous] // acceso libre
 public async Task<IActionResult> Login([FromBody] LoginRequest request)
 {
 var result = await _auth.LoginAsync(request);

 if (result == null)
 return Unauthorized(new { message = "Credenciales inválidas" });

 return Ok(result);
 }

 // POST: api/auth/refresh
 [HttpPost("refresh")]
 [AllowAnonymous] // acceso libre (token expirado)
 public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
 {
 var result = await _auth.RefreshTokenAsync(request);

 if (result == null)
 return Unauthorized(new { message = "Token inválido o expirado" });

 return Ok(result);
 }

 // POST: api/auth/revoke
 [HttpPost("revoke")]
 [Authorize] // requiere JWT válido
 public async Task<IActionResult> Revoke()
 {
 // Obtiene el usuario actual desde HttpContext o Claims del JWT
 int usuarioId =0;

 if (HttpContext.Items.TryGetValue("UsuarioId", out var idObj) && idObj is int idFromMiddleware)
 {
 usuarioId = idFromMiddleware;
 }
 else
 {
 var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
 ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
 ?? User.Identity?.Name;

 if (string.IsNullOrWhiteSpace(idClaim) || !int.TryParse(idClaim, out usuarioId))
 {
 return Unauthorized(new { message = "No se pudo determinar el usuario actual" });
 }
 }

 var ok = await _auth.RevokeRefreshTokenAsync(usuarioId);

 if (!ok)
 return BadRequest(new { message = "No se pudo revocar el token" });

 return Ok(new { message = "Refresh token revocado" });
 }
 }
}
