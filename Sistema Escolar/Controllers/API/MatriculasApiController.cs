using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using SistemaEscolar.DTOs.Matriculas;
using System.Linq;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/matriculas")]
 [Authorize(Roles = "Administrador")] // solo administradores pueden matricular
 public class MatriculasApiController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 public MatriculasApiController(ApplicationDbContext ctx){ _ctx = ctx; }

 // Backwards-compatible single-course DTO (kept as inner class for compatibility)
 public class MatriculaSingleDTO { public int EstudianteId { get; set; } public int CursoId { get; set; } public int CuatrimestreId { get; set; } public int CursoOfertaId { get; set; } }

 [HttpPost]
 public async Task<IActionResult> Create([FromBody] MatriculaCreateDTO? batchDto, [FromBody] MatriculaSingleDTO? singleDto)
 {
 // Determine which DTO was provided. Prefer batchDto if present.
 if (batchDto == null && singleDto == null)
 {
 return BadRequest(new { message = "Payload inválido" });
 }

 // If singleDto provided (legacy clients), convert to batch form
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
 if (!ModelState.IsValid) return BadRequest(ModelState);

 var dto = batchDto!;

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
 if (existing.Any()) return Conflict(new { message = "Ya existe matrícula para algunos cursos seleccionados" });

 // crear matriculas
 var now = DateTime.UtcNow;
 var list = dto.CursosIds.Select(cid => new Models.Academico.Matricula { EstudianteId = dto.EstudianteId, CuatrimestreId = dto.CuatrimestreId, CursoId = cid, FechaMatricula = now, Activo = true }).ToList();
 _ctx.Matriculas.AddRange(list);
 await _ctx.SaveChangesAsync();

 return Ok(new { message = "Matrícula guardada", created = list.Count });
 }
 }
}
