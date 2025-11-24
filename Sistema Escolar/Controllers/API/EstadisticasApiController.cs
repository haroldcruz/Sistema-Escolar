using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/estadisticas")]
 [Authorize]
 public class EstadisticasApiController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 public EstadisticasApiController(ApplicationDbContext ctx) { _ctx = ctx; }

 // GET api/estadisticas?cuatrimestreId=1&cursoId=2
 [HttpGet]
 public async Task<IActionResult> Get([FromQuery] int cuatrimestreId, [FromQuery] int cursoId)
 {
 // validar existencia
 var curso = await _ctx.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cursoId && c.CuatrimestreId == cuatrimestreId);
 if (curso == null) return NotFound(new { message = "Curso no encontrado para el cuatrimestre" });

 // Si el usuario es docente, asegurar que esté asignado al curso
 var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
 if (!string.IsNullOrEmpty(uid) && User.IsInRole("Docente"))
 {
 var docenteId = int.Parse(uid);
 var asign = await _ctx.CursoDocentes.AnyAsync(cd => cd.CursoId == cursoId && cd.DocenteId == docenteId && cd.Activo);
 if (!asign) return Forbid();
 }

 // total matriculados en ese curso y cuatrimestre
 var totalMatriculados = await _ctx.Matriculas.Where(m => m.CursoId == cursoId && m.CuatrimestreId == cuatrimestreId && m.Activo).Select(m => m.Id).Distinct().CountAsync();

 // evaluaciones relacionadas
 var evals = _ctx.Evaluaciones.AsNoTracking().Where(e => e.Matricula != null && e.Matricula.CursoId == cursoId && e.Matricula.CuatrimestreId == cuatrimestreId);

 var totalConEvaluacion = await evals.Select(e => e.MatriculaId).Distinct().CountAsync();
 var totalAprobados = await evals.Where(e => e.Estado.ToLower() == "aprobado").Select(e => e.MatriculaId).Distinct().CountAsync();
 var totalReprobados = await evals.Where(e => e.Estado.ToLower() == "reprobado").Select(e => e.MatriculaId).Distinct().CountAsync();

 double porcentajeParticipacion = totalMatriculados ==0 ?0 : (double)totalConEvaluacion / totalMatriculados *100.0;
 double porcentajeAprobados = totalMatriculados ==0 ?0 : (double)totalAprobados / totalMatriculados *100.0;
 double porcentajeReprobados = totalMatriculados ==0 ?0 : (double)totalReprobados / totalMatriculados *100.0;

 return Ok(new
 {
 curso = new { id = curso.Id, codigo = curso.Codigo, nombre = curso.Nombre },
 cuatrimestre = curso.CuatrimestreId,
 totalMatriculados,
 totalConEvaluacion,
 totalAprobados,
 totalReprobados,
 porcentajeParticipacion = System.Math.Round(porcentajeParticipacion,2),
 porcentajeAprobados = System.Math.Round(porcentajeAprobados,2),
 porcentajeReprobados = System.Math.Round(porcentajeReprobados,2)
 });
 }
 }
}
