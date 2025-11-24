using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using SistemaEscolar.Data;
using SistemaEscolar.Helpers;
using Microsoft.EntityFrameworkCore;

namespace SistemaEscolar.Controllers.Dev
{
 // Solo disponible en entorno Development
 [ApiController]
 [Route("dev/reset-password")]
 public class DevResetController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 private readonly IHostEnvironment _env;
 public DevResetController(ApplicationDbContext ctx, IHostEnvironment env){ _ctx = ctx; _env = env; }

 [HttpPost]
 public async Task<IActionResult> Post([FromBody] ResetRequest req)
 {
 if (!_env.IsDevelopment()) return NotFound(); // oculto fuera de dev
 if (string.IsNullOrWhiteSpace(req?.Email) || string.IsNullOrWhiteSpace(req?.Password)) return BadRequest(new { message = "Email y Password requeridos" });

 var user = await _ctx.Usuarios.FirstOrDefaultAsync(u => u.Email == req.Email);
 if (user == null) return NotFound(new { message = "Usuario no encontrado" });

 PasswordHasher.CreatePasswordHash(req.Password, out var hash, out var salt);
 user.PasswordHash = hash;
 user.PasswordSalt = salt;
 await _ctx.SaveChangesAsync();
 return Ok(new { message = "Password restablecida (dev)" });
 }

 public class ResetRequest{ public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
 }
}
