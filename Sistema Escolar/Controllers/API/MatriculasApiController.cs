using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using SistemaEscolar.DTOs.Matriculas;
using System.Linq;
using System.Text.Json;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/matriculas")]
 [Authorize(Roles = "Administrador")] // solo administradores pueden matricular
 public class MatriculasApiController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 public MatriculasApiController(ApplicationDbContext ctx){ _ctx = ctx; }

 // GET api/matriculas?estudianteId=3&page=1&pageSize=10
 [HttpGet]
 public async Task<IActionResult> GetByEstudiante([FromQuery] int estudianteId, [FromQuery] int page =1, [FromQuery] int pageSize =10)
 {
 if (estudianteId <=0) return BadRequest(new { message = "Estudiante inválido" });
 if (page <=0) page =1;
 if (pageSize <=0 || pageSize >200) pageSize =10;

 var exists = await _ctx.Usuarios.AnyAsync(u => u.Id == estudianteId);
 if (!exists) return NotFound(new { message = "Estudiante no encontrado" });

 var query = _ctx.Matriculas.AsNoTracking()
 .Where(m => m.EstudianteId == estudianteId)
 .Include(m => m.Curso)
 .Include(m => m.Cuatrimestre)
 .OrderByDescending(m => m.FechaMatricula);

 var total = await query.CountAsync();
 var items = await query.Skip((page -1) * pageSize).Take(pageSize)
 .Select(m => new {
 m.Id,
 CursoId = m.CursoId,
 Curso = m.Curso != null ? m.Curso.Nombre : string.Empty,
 m.CuatrimestreId,
 Cuatrimestre = m.Cuatrimestre != null ? m.Cuatrimestre.Nombre : string.Empty,
 FechaMatricula = m.FechaMatricula,
 m.Activo
 })
 .ToListAsync();

 return Ok(new { total, page, pageSize, items });
 }

 // Backwards-compatible single-course DTO (kept as inner class for compatibility)
 public class MatriculaSingleDTO { public int EstudianteId { get; set; } public int CursoId { get; set; } public int CuatrimestreId { get; set; } public int CursoOfertaId { get; set; } }

 [HttpPost]
 public async Task<IActionResult> Create([FromBody] JsonElement payload)
 {
 MatriculaCreateDTO? batchDto = null;
 MatriculaSingleDTO? singleDto = null;
 var options = new JsonSerializerOptions{ PropertyNameCaseInsensitive = true };

 // Try to deserialize as batch DTO
 try{ batchDto = JsonSerializer.Deserialize<MatriculaCreateDTO>(payload.GetRawText(), options); } catch { batchDto = null; }
 // If not batch, try single
 if(batchDto == null)
 {
 try{ singleDto = JsonSerializer.Deserialize<MatriculaSingleDTO>(payload.GetRawText(), options); } catch { singleDto = null; }
 }

 if (batchDto == null && singleDto == null)
 {
 return BadRequest(new { message = "Payload inválido" });
 }

 if (batchDto == null && singleDto != null)
 {
 batchDto = new MatriculaCreateDTO
 {
 EstudianteId = singleDto.EstudianteId,
 CuatrimestreId = singleDto.CuatrimestreId,
 CursosIds = new System.Collections.Generic.List<int> { singleDto.CursoId }
 };
 }

 // Validate model server-side
 if (batchDto!.EstudianteId <=0) return BadRequest(new { message = "Estudiante inválido" });
 if (batchDto.CuatrimestreId <=0) return BadRequest(new { message = "Cuatrimestre inválido" });
 if (batchDto.CursosIds == null || !batchDto.CursosIds.Any()) return BadRequest(new { message = "Debe seleccionar al menos un curso" });

 var dto = batchDto;

 // validar existencia estudiante y cuatrimestre
 var estudiante = await _ctx.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == dto.EstudianteId);
 if (estudiante == null) return NotFound(new { message = "Estudiante no encontrado" });
 var cuatr = await _ctx.Cuatrimestres.AsNoTracking().FirstOrDefaultAsync(c => c.Id == dto.CuatrimestreId);
 if (cuatr == null) return NotFound(new { message = "Cuatrimestre no encontrado" });

 // traer cursos y validar que pertenecen al cuatrimestre
 var cursos = await _ctx.Cursos.Where(c => dto.CursosIds.Contains(c.Id)).ToListAsync();
 if (cursos.Count != dto.CursosIds.Count) return BadRequest(new { message = "Algunos cursos seleccionados no existen" });
 if (cursos.Any(c => c.CuatrimestreId != dto.CuatrimestreId)) return BadRequest(new { message = "Algunos cursos seleccionados no pertenecen al cuatrimestre" });

 // prevenir duplicados: estudiante + cuatrimestre + curso
 var existing = await _ctx.Matriculas.Where(m => m.EstudianteId == dto.EstudianteId && m.CuatrimestreId == dto.CuatrimestreId && dto.CursosIds.Contains(m.CursoId)).ToListAsync();
 if (existing.Any())
 {
 // Build conflict details: course ids and names
 var conflictCourseIds = existing.Select(m => m.CursoId).Distinct().ToList();
 var conflictCourses = await _ctx.Cursos.Where(c => conflictCourseIds.Contains(c.Id)).Select(c => new { c.Id, c.Codigo, c.Nombre }).ToListAsync();
 return Conflict(new { message = "Ya existe matrícula para algunos cursos seleccionados", conflicts = conflictCourses });
 }

 // crear matriculas
 var now = DateTime.UtcNow;
 var list = dto.CursosIds.Select(cid => new Models.Academico.Matricula { EstudianteId = dto.EstudianteId, CuatrimestreId = dto.CuatrimestreId, CursoId = cid, FechaMatricula = now, Activo = true }).ToList();
 _ctx.Matriculas.AddRange(list);
 await _ctx.SaveChangesAsync();

 return Ok(new { message = "Matrícula guardada", created = list.Count });
 }
 }
}
