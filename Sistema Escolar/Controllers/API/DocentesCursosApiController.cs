using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.Interfaces.Cursos;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/docentes/{docenteId:int}")]
 [Authorize(Policy = "Cursos.AsignarDocente")]
 public class DocentesCursosApiController : ControllerBase
 {
 private readonly ICursoService _cursos;
 private readonly ApplicationDbContext _ctx;
 public DocentesCursosApiController(ICursoService cursos, ApplicationDbContext ctx){ _cursos = cursos; _ctx = ctx; }

 [HttpGet("cursos")]
 public async Task<IActionResult> GetCursosAsignados(int docenteId){
 if(!await _ctx.Usuarios.AnyAsync(u=>u.Id==docenteId)) return NotFound();
 var list = await _cursos.GetCursosDeDocenteAsync(docenteId);
 return Ok(list);
 }

 [HttpGet("cursos-disponibles")]
 public async Task<IActionResult> GetCursosDisponibles(int docenteId, [FromQuery] int? cuatrimestreId){
 if(!await _ctx.Usuarios.AnyAsync(u=>u.Id==docenteId)) return NotFound();
 var list = await _cursos.GetCursosDisponiblesParaDocenteAsync(docenteId, cuatrimestreId);
 return Ok(list);
 }
 }
}
