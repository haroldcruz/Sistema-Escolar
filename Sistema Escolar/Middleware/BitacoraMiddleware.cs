using Microsoft.AspNetCore.Http;
using SistemaEscolar.Interfaces.Bitacora;
using System.Threading.Tasks;
using System.Linq;

namespace SistemaEscolar.Middleware
{
 // Middleware que registra todas las acciones del usuario
 public class BitacoraMiddleware
 {
 private readonly RequestDelegate _next;

 public BitacoraMiddleware(RequestDelegate next)
 {
 _next = next;
 }

 public async Task InvokeAsync(HttpContext context, IBitacoraService bitacoraService)
 {
 // Dejar pasar la petición primero
 await _next(context);

 try
 {
 // Solo registrar si el usuario está autenticado
 if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
 {
 // Id del usuario
 var userIdClaim = context.User.Claims
 .FirstOrDefault(x => x.Type == "sub" || x.Type.Contains("nameidentifier"));

 if (userIdClaim == null)
 return;

 var usuarioId = int.Parse(userIdClaim.Value);

 // Acción: método HTTP
 var accion = context.Request.Method;

 // Módulo: ruta
 var modulo = context.Request.Path.ToString();

 // IP del usuario
 var ip = context.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

 // Registrar en BD
 await bitacoraService.RegistrarAsync(usuarioId, accion, modulo, ip);
 }
 }
 catch
 {
 // Silenciar errores de bitácora para no afectar la app
 }
 }
 }
}
