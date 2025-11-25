using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Cursos;
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
 IQueryable<SistemaEscolar.Models.Academico.Curso> query = _ctx.Cursos.AsNoTracking()
 .Include(c => c.Cuatrimestre)
 .Include(c => c.CursoDocentes).ThenInclude(cd => cd.Docente)
 ;
 if (cuatrimestreId.HasValue)
 {
 query = query.Where(c => c.CuatrimestreId == cuatrimestreId.Value);
 }
 var list = await query.OrderBy(c => c.Codigo).Select(c => new {
 id = c.Id,
 codigo = c.Codigo,
 nombre = string.IsNullOrWhiteSpace(c.Nombre) ? c.Codigo : (c.Codigo + " - " + c.Nombre),
 creditos = c.Creditos,
 cuatrimestre = c.Cuatrimestre != null ? c.Cuatrimestre.Nombre : string.Empty,
 docentes = c.CursoDocentes
 .Select(cd => ((cd.Docente != null ? (cd.Docente.Nombre ?? string.Empty) : string.Empty)
 + " " + (cd.Docente != null ? (cd.Docente.Apellidos ?? string.Empty) : string.Empty)).Trim())
 .Where(s => !string.IsNullOrEmpty(s)).ToList()
 }).ToListAsync();
 return Ok(list);
 }

 // GET api/cursos/{cursoId}/alumnos
 [HttpGet("{cursoId:int}/alumnos")]
 public async Task<IActionResult> GetAlumnos(int cursoId, [FromQuery] int? cuatrimestreId)
 {
 if (!await _ctx.Cursos.AnyAsync(c => c.Id == cursoId)) return NotFound(new { message = "Curso no encontrado" });
 var q = _ctx.Matriculas.AsNoTracking().Where(m => m.CursoId == cursoId && m.Activo);
 if (cuatrimestreId.HasValue) q = q.Where(m => m.CuatrimestreId == cuatrimestreId.Value);
 var list = await q.Include(m => m.Estudiante).OrderBy(m => m.Estudiante.Nombre).Select(m => new { id = m.Id, nombre = (m.Estudiante.Nombre + " " + m.Estudiante.Apellidos).Trim() }).ToListAsync();
 return Ok(list);
 }

 // POST api/cursos/{cursoId}/notaFinal
 [HttpPost("{cursoId:int}/notaFinal")]
 [Authorize(Roles = "Docente,Coordinador,Administrador")]
 public async Task<IActionResult> PostNotaFinal(int cursoId, [FromBody] NotaFinalCreateDTO dto)
 {
 if (dto == null || dto.Items == null || !dto.Items.Any()) return BadRequest(new { message = "Payload inválido" });
 var curso = await _ctx.Cursos.FindAsync(cursoId);
 if (curso == null) return NotFound(new { message = "Curso no encontrado" });
 // Obtener matriculas válidas para curso y cuatrimestre
 var matriculasQuery = _ctx.Matriculas.Where(m => m.CursoId == cursoId && m.Activo);
 if (dto.CuatrimestreId.HasValue) matriculasQuery = matriculasQuery.Where(m => m.CuatrimestreId == dto.CuatrimestreId.Value);
 var matriculas = await matriculasQuery.Select(m => m.Id).ToListAsync();
 // Filtrar items por matriculas existentes
 var items = dto.Items.Where(i => matriculas.Contains(i.MatriculaId)).ToList();
 if (!items.Any()) return BadRequest(new { message = "No hay matrículas válidas en los items" });
 // Guardar como Evaluacion final: crear o actualizar registro en Evaluaciones con Participacion='Final' y Observaciones empty
 using var trx = await _ctx.Database.BeginTransactionAsync();
 try
 {
 var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value; int uid =0;
 if (!string.IsNullOrEmpty(userId)) int.TryParse(userId, out uid);
 foreach (var it in items)
 {
 // Buscar evaluación existente marcada como final para esa matrícula (participacion == 'Final')
 var existing = await _ctx.Evaluaciones.FirstOrDefaultAsync(e => e.MatriculaId == it.MatriculaId && e.Participacion == "Final");
 if (existing == null)
 {
 var ev = new SistemaEscolar.Models.Academico.Evaluacion
 {
 MatriculaId = it.MatriculaId,
 Nota = it.NotaFinal,
 Estado = it.NotaFinal >=50 ? "Aprobado" : "Reprobado",
 Observaciones = "Nota final automática",
 Participacion = "Final",
 UsuarioRegistro = uid
 };
 _ctx.Evaluaciones.Add(ev);
 }
 else
 {
 existing.Nota = it.NotaFinal;
 existing.Estado = it.NotaFinal >=50 ? "Aprobado" : "Reprobado";
 existing.Observaciones = "Nota final actualizada";
 existing.UsuarioRegistro = uid;
 _ctx.Evaluaciones.Update(existing);
 }
 }
 await _ctx.SaveChangesAsync();
 await trx.CommitAsync();
 return Ok(new { message = "Notas finales registradas" });
 }
 catch (System.Exception ex)
 {
 await trx.RollbackAsync();
 return StatusCode(500, new { message = "Error al guardar notas finales", detail = ex.Message });
 }
 }
 }
}
