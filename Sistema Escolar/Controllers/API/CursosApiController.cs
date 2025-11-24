using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/cursos")]
 [Authorize]
 public class CursosApiController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 public CursosApiController(ApplicationDbContext ctx) { _ctx = ctx; }

 [HttpGet]
 public async Task<IActionResult> Get([FromQuery] int? cuatrimestreId)
 {
 var query = _ctx.Cursos.AsNoTracking();
 if (cuatrimestreId.HasValue)
 {
 query = query.Where(c => c.CuatrimestreId == cuatrimestreId.Value);
 }
 var list = await query.OrderBy(c => c.Nombre).Select(c => new { c.Id, c.Codigo, Nombre = c.Codigo + " - " + c.Nombre }).ToListAsync();
 return Ok(list);
 }
 }
}
