using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Interfaces.Cursos;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/cursos/{cursoId:int}/horarios")]
 [Authorize(Policy = "Cursos.Editar")]
 public class HorariosApiController : ControllerBase
 {
 private readonly ICursoService _cursos;
 public HorariosApiController(ICursoService cursos){ _cursos = cursos; }
 private int CurrentUserId()=> int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
 private string Ip()=> HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

 [HttpGet]
 public async Task<IActionResult> Get(int cursoId){
 var list = await _cursos.GetHorariosAsync(cursoId);
 return Ok(list);
 }

 public record HorarioReq(int DiaSemana, string Inicio, string Fin);

 [HttpPost]
 public async Task<IActionResult> Add(int cursoId, [FromBody] HorarioReq req){
 if(!TimeSpan.TryParse(req.Inicio, out var inicio) || !TimeSpan.TryParse(req.Fin, out var fin))
 return BadRequest(new{ message="Formato de hora inválido"});
 var (ok, error) = await _cursos.AddHorarioAsync(cursoId, req.DiaSemana, inicio, fin, CurrentUserId(), Ip());
 if(!ok) return BadRequest(new{ message = error ?? "No se pudo agregar"});
 return Ok(new{ message="Horario agregado"});
 }

 [HttpDelete("{horarioId:int}")]
 public async Task<IActionResult> Remove(int cursoId, int horarioId){
 var ok = await _cursos.RemoveHorarioAsync(horarioId, CurrentUserId(), Ip());
 if(!ok) return NotFound(new{ message="No encontrado"});
 return Ok(new{ message="Horario eliminado"});
 }
 }
}
