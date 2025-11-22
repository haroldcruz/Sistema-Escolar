using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace SistemaEscolar.Middleware
{
 // Middleware para exponer el IdUsuario desde el JWT
 public class JwtMiddleware
 {
 private readonly RequestDelegate _next;

 public JwtMiddleware(RequestDelegate next)
 {
 _next = next;
 }

 public async Task InvokeAsync(HttpContext context)
 {
 // Si el usuario está autenticado, se extrae el Id y se expone en Items
 if (context.User?.Identity != null && context.User.Identity.IsAuthenticated)
 {
 var idValue = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
 ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
 ?? context.User.FindFirst(ClaimTypes.Sid)?.Value
 ?? context.User.Identity.Name;

 if (!string.IsNullOrWhiteSpace(idValue) && int.TryParse(idValue, out var userId))
 {
 context.Items["UsuarioId"] = userId;
 }
 }

 await _next(context);
 }
 }
}
