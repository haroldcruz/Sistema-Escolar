using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using SistemaEscolar.Data;using SistemaEscolar.Helpers;using System.Linq;using System.Threading.Tasks;using Microsoft.EntityFrameworkCore;
namespace SistemaEscolar.Controllers
{
 [Authorize(Policy="Seguridad.Gestion")] // solo seguridad
 [Route("Seguridad/[controller]")]
 public class ResetController : Controller
 {
 private readonly ApplicationDbContext _ctx;
 public ResetController(ApplicationDbContext ctx){ _ctx = ctx; }

 // POST: /Seguridad/Reset/admin-password
 [HttpPost("admin-password")] public async Task<IActionResult> ResetAdminPassword(){
 var admin = await _ctx.Usuarios.FirstOrDefaultAsync(u=>u.Email=="admin@sistema.edu");
 if(admin==null) return NotFound();
 PasswordHasher.CreatePasswordHash("Admin123!", out var hash, out var salt);
 admin.PasswordHash = hash; admin.PasswordSalt = salt;
 await _ctx.SaveChangesAsync();
 return Ok(new{ message="Password admin restablecido" }); }
 }
}
