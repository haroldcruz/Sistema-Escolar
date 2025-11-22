using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace SistemaEscolar.Middleware
{
 // Maneja excepciones globales y devuelve JSON limpio
 public class ErrorHandlerMiddleware
 {
 private readonly RequestDelegate _next;

 public ErrorHandlerMiddleware(RequestDelegate next)
 {
 _next = next;
 }

 public async Task InvokeAsync(HttpContext context)
 {
 try
 {
 await _next(context); // continuar
 }
 catch (System.Exception ex)
 {
 context.Response.ContentType = "application/json";
 context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

 var json = new
 {
 message = "Error interno del servidor",
 detalle = ex.Message
 };

 await context.Response.WriteAsync(JsonSerializer.Serialize(json));
 }
 }
 }
}
