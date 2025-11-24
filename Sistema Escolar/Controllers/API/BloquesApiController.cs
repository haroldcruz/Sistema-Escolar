using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Bloques;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Claims;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/bloques")]
 [Authorize(Roles = "Docente,Coordinador,Administrador")]
 public class BloquesApiController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 public BloquesApiController(ApplicationDbContext ctx){ _ctx = ctx; }

 [HttpPost]
 public async Task<IActionResult> Create([FromBody] BloqueCreateDTO dto)
 {
 if(!ModelState.IsValid) return BadRequest(ModelState);
 var curso = await _ctx.Cursos.FirstOrDefaultAsync(c => c.Id == dto.CursoId && c.CuatrimestreId == dto.CuatrimestreId);
 if(curso == null) return BadRequest(new { message = "Curso no encontrado para el cuatrimestre" });
 var uidStr = User.FindFirstValue(ClaimTypes.NameIdentifier); int uid =0; if(!string.IsNullOrEmpty(uidStr)) uid = int.Parse(uidStr);
 if(User.IsInRole("Docente")){
 var asign = await _ctx.CursoDocentes.AnyAsync(cd => cd.CursoId == dto.CursoId && cd.DocenteId == uid && cd.Activo);
 if(!asign) return Forbid();
 }
 var b = new Models.Academico.BloqueEvaluacion
 {
 CursoId = dto.CursoId,
 CuatrimestreId = dto.CuatrimestreId,
 Nombre = dto.Nombre,
 Tipo = dto.Tipo,
 Peso = dto.Peso,
 FechaCreacion = DateTime.UtcNow,
 CreadoPorId = uid
 };
 _ctx.BloqueEvaluaciones.Add(b);
 await _ctx.SaveChangesAsync();
 return Ok(new { message = "Bloque creado", id = b.Id });
 }

 [HttpGet]
 public async Task<IActionResult> List([FromQuery] int cursoId, [FromQuery] int cuatrimestreId)
 {
 var list = await _ctx.BloqueEvaluaciones
  .Where(b => b.CursoId == cursoId && b.CuatrimestreId == cuatrimestreId)
  .OrderByDescending(b => b.FechaCreacion)
  .Select(b => new { b.Id, b.Nombre, b.Tipo, b.Peso, b.FechaCreacion })
  .ToListAsync();
 return Ok(list);
 }

 [HttpGet("{id:int}/alumnos")]
 public async Task<IActionResult> GetAlumnos(int id)
 {
 var bloque = await _ctx.BloqueEvaluaciones.FindAsync(id);
 if(bloque==null) return NotFound(new{ message = "Bloque no encontrado" });
 var mats = await _ctx.Matriculas
  .Where(m => m.CursoId == bloque.CursoId && m.CuatrimestreId == bloque.CuatrimestreId && m.Activo)
  .Include(m => m.Estudiante)
  .Select(m => new { m.Id, Nombre = (m.Estudiante.Nombre + " " + m.Estudiante.Apellidos).Trim() })
  .ToListAsync();
 return Ok(mats);
 }

 [HttpPost("{id:int}/calificaciones")]
 public async Task<IActionResult> Calificar(int id, [FromBody] CalificacionCreateDTO dto)
 {
 if(!ModelState.IsValid) return BadRequest(ModelState);
 if(dto.Items == null || !dto.Items.Any()) return BadRequest(new { message = "Debe proporcionar al menos un item para calificar" });
 var bloque = await _ctx.BloqueEvaluaciones.FindAsync(id);
 if(bloque==null) return NotFound(new{ message = "Bloque no encontrado" });
 var uidStr = User.FindFirstValue(ClaimTypes.NameIdentifier); int uid =0; if(!string.IsNullOrEmpty(uidStr)) uid = int.Parse(uidStr);
 if(User.IsInRole("Docente")){
 var asign = await _ctx.CursoDocentes.AnyAsync(cd => cd.CursoId == bloque.CursoId && cd.DocenteId == uid && cd.Activo);
 if(!asign) return Forbid();
 }
 var matriculaIds = dto.Items.Select(i=>i.MatriculaId).Distinct().ToList();
 var exists = await _ctx.CalificacionBloques.Where(cb => cb.BloqueEvaluacionId==id && matriculaIds.Contains(cb.MatriculaId)).Select(cb=>cb.MatriculaId).ToListAsync();
 if(exists.Any()) return Conflict(new{ message = "Ya existen calificaciones para algunos estudiantes en este bloque", conflicts = exists });
 var now = DateTime.UtcNow;
 var list = dto.Items.Select(i => new Models.Academico.CalificacionBloque
 {
 BloqueEvaluacionId = id,
 MatriculaId = i.MatriculaId,
 Nota = i.Nota,
 Observaciones = i.Observaciones,
 Estado = i.Estado,
 FechaRegistro = now,
 UsuarioRegistro = uid
 }).ToList();
 _ctx.CalificacionBloques.AddRange(list);
 await _ctx.SaveChangesAsync();
 return Ok(new{ message = "Calificaciones registradas", created = list.Count });
 }
 }
}
