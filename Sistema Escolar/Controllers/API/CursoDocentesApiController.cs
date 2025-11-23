using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.Interfaces.Cursos;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/cursos/{cursoId:int}/docentes")]
 [Authorize(Policy = "Cursos.AsignarDocente")]
 public class CursoDocentesApiController : ControllerBase
 {
 private readonly ICursoService _cursos;
 private readonly ApplicationDbContext _ctx;
 public CursoDocentesApiController(ICursoService cursos, ApplicationDbContext ctx){ _cursos = cursos; _ctx = ctx; }
 private int CurrentUserId()=> int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
 private string Ip()=> HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

 [HttpGet]
 public async Task<IActionResult> Get(int cursoId){
 if(!await _ctx.Cursos.AnyAsync(c=>c.Id==cursoId)) return NotFound(new{message="Curso no encontrado"});
 var lista = await _cursos.GetDocentesAsignadosAsync(cursoId);
 return Ok(lista);
 }

 [HttpPost("{docenteId:int}")]
 public async Task<IActionResult> Asignar(int cursoId, int docenteId){
 var (ok, error) = await _cursos.AsignarDocenteAsync(cursoId, docenteId, CurrentUserId(), Ip());
 if(!ok) return BadRequest(new{ message = error ?? "No se pudo asignar"});
 return Ok(new{ message = "Docente asignado"});
 }

 [HttpDelete("{docenteId:int}")]
 public async Task<IActionResult> Quitar(int cursoId, int docenteId){
 var ok = await _cursos.QuitarDocenteAsync(cursoId, docenteId, CurrentUserId(), Ip());
 if(!ok) return NotFound(new{ message = "No encontrado"});
 return Ok(new{ message = "Docente quitado"});
 }
 }
}
