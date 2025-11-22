using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SistemaEscolar.Middleware
{
 // Middleware desactivado: ya usamos JwtBearer que configura HttpContext.User
 public class JwtMiddleware
 {
 private readonly RequestDelegate _next;
 public JwtMiddleware(RequestDelegate next){ _next = next; }
 public async Task InvokeAsync(HttpContext context){
 // No manipular principal; dejar a JwtBearer
 await _next(context);
 }
 }
}
