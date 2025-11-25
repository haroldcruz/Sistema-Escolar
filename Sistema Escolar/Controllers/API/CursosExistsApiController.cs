using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/cursos")]
 [Authorize]
 public class CursosExistsApiController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 public CursosExistsApiController(ApplicationDbContext ctx){ _ctx = ctx; }

 // GET api/cursos/exists?codigo=...
 [HttpGet("exists")]
 public async Task<IActionResult> Exists([FromQuery] string codigo){
 if (string.IsNullOrWhiteSpace(codigo)) return BadRequest(new { ok = false, message = "Código requerido" });
 var exists = await _ctx.Cursos.AnyAsync(c => c.Codigo == codigo);
 return Ok(new { ok = true, data = new { exists } });
 }
 }
}
