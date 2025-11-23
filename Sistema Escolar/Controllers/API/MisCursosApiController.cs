using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Cursos;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/mis-cursos")]
 [Authorize]
 public class MisCursosApiController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 public MisCursosApiController(ApplicationDbContext ctx){ _ctx = ctx; }

 [HttpGet]
 public async Task<IActionResult> Get()
 {
 var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
 if (uid ==0) return Unauthorized();
 var cursos = await _ctx.Cursos.Include(c => c.Cuatrimestre).Include(c => c.CursoDocentes).ThenInclude(cd => cd.Docente)
 .Where(c => c.CursoDocentes.Any(cd => cd.DocenteId == uid))
 .ToListAsync();
 var dto = cursos.Select(c => new CursoDTO { Id = c.Id, Codigo = c.Codigo, Nombre = c.Nombre, Creditos = c.Creditos, Cuatrimestre = c.Cuatrimestre?.Nombre ?? string.Empty, Docentes = c.CursoDocentes.Select(cd => (cd.Docente.Nombre + " " + cd.Docente.Apellidos).Trim()).ToList() });
 return Ok(dto);
 }
 }
}
