using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Bloques;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Claims;
using SistemaEscolar.Models.Academico;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/bloques")]
 public class BloquesApiController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 public BloquesApiController(ApplicationDbContext ctx){ _ctx = ctx; }

 // GET list - allow authenticated docentes and admins
 [HttpGet]
 [Authorize(Roles = "Docente,Administrador")]
 public async Task<IActionResult> List([FromQuery] int cursoId, [FromQuery] int cuatrimestreId)
 {
 var list = await _ctx.BloqueEvaluaciones
 .Where(b => b.CursoId == cursoId && b.CuatrimestreId == cuatrimestreId)
 .OrderByDescending(b => b.FechaCreacion)
 .Select(b => new { b.Id, b.Nombre, b.Tipo, b.Peso, b.FechaCreacion })
 .ToListAsync();
 return Ok(list);
 }

 // GET alumnos for a bloque - allow docentes and admins but verify docente assignment elsewhere if needed
 [HttpGet("{id:int}/alumnos")]
 [Authorize(Roles = "Docente,Administrador")]
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

 // GET fechas
 [HttpGet("{id:int}/fechas")]
 [Authorize(Roles = "Docente,Administrador")]
 public async Task<IActionResult> GetFechas(int id)
 {
 var bloque = await _ctx.BloqueEvaluaciones.FindAsync(id);
 if(bloque == null) return NotFound(new { message = "Bloque no encontrado" });
 var fechas = await _ctx.BloqueFechas.Where(bf => bf.BloqueEvaluacionId == id).OrderBy(bf => bf.Fecha).Select(bf => new { bf.Id, bf.Fecha }).ToListAsync();
 return Ok(fechas);
 }

 // GET calificaciones
 [HttpGet("{id:int}/calificaciones")]
 [Authorize(Roles = "Docente,Administrador")]
 public async Task<IActionResult> GetCalificaciones(int id)
 {
 var bloque = await _ctx.BloqueEvaluaciones.FindAsync(id);
 if(bloque == null) return NotFound(new { message = "Bloque no encontrado" });
 var cals = await _ctx.CalificacionBloques.Where(cb => cb.BloqueEvaluacionId == id).Include(cb => cb.Matricula).ThenInclude(m => m.Estudiante).Select(cb => new { cb.MatriculaId, Nombre = cb.Matricula.Estudiante.Nombre + " " + cb.Matricula.Estudiante.Apellidos, cb.Nota, cb.Estado, cb.Observaciones, Asistencias = bloque.Tipo == "Asistencia" ? _ctx.AsistenciaBloques.Where(ab => ab.CalificacionBloqueId == cb.Id).Select(ab => ab.BloqueFechaId).ToList() : null }).ToListAsync();
 return Ok(cals);
 }

 // POST create - allow Admin and Docente (docente must be assigned to the curso)
 [HttpPost]
 [Authorize(Roles = "Docente,Administrador")]
 public async Task<IActionResult> Create([FromBody] BloqueCreateDTO dto)
 {
 if(!ModelState.IsValid) return BadRequest(ModelState);
 var curso = await _ctx.Cursos.FirstOrDefaultAsync(c => c.Id == dto.CursoId && c.CuatrimestreId == dto.CuatrimestreId);
 if(curso == null) return BadRequest(new { message = "Curso no encontrado para el cuatrimestre" });
 var uidStr = User.FindFirstValue(ClaimTypes.NameIdentifier); int uid =0; if(!string.IsNullOrEmpty(uidStr)) uid = int.Parse(uidStr);

 // Si el usuario es Docente, verificar que esté asignado al curso y activo
 if (User.IsInRole("Docente"))
 {
 var asignado = await _ctx.CursoDocentes.AnyAsync(cd => cd.CursoId == dto.CursoId && cd.DocenteId == uid && cd.Activo);
 if (!asignado) return Forbid();
 }
 // Administrador puede crear sin verificación adicional

 // Validar suma de pesos
 if(dto.Peso.HasValue){
 var sumaPesos = await _ctx.BloqueEvaluaciones.Where(b => b.CursoId == dto.CursoId && b.CuatrimestreId == dto.CuatrimestreId && b.Peso.HasValue).SumAsync(b => b.Peso.Value);
 if(sumaPesos + dto.Peso.Value >100) return BadRequest(new { message = "La suma de los pesos de los bloques no puede exceder100%" });
 }
 var b = new Models.Academico.BloqueEvaluacion
 {
 CursoId = dto.CursoId,
 CuatrimestreId = dto.CuatrimestreId,
 Nombre = dto.Nombre,
 Tipo = dto.Tipo,
 Peso = dto.Peso,
 FechaCreacion = DateTime.UtcNow,
 FechaAsignacion = dto.FechaAsignacion,
 CreadoPorId = uid
 };
 _ctx.BloqueEvaluaciones.Add(b);
 await _ctx.SaveChangesAsync();
 if(dto.Tipo == "Asistencia" && dto.FechasAsistencia != null && dto.FechasAsistencia.Any()){
 var fechas = dto.FechasAsistencia.Select(f => new Models.Academico.BloqueFecha { BloqueEvaluacionId = b.Id, Fecha = f }).ToList();
 _ctx.BloqueFechas.AddRange(fechas);
 await _ctx.SaveChangesAsync();
 }
 return Ok(new { message = "Bloque creado", id = b.Id });
 }

 // POST calificaciones - allow Docente (if assigned) and Admin
 [HttpPost("{id:int}/calificaciones")]
 [Authorize(Roles = "Docente,Administrador")]
 public async Task<IActionResult> Calificar(int id, [FromBody] CalificacionCreateDTO dto)
 {
 if(!ModelState.IsValid) return BadRequest(ModelState);
 if(dto.Items == null || !dto.Items.Any()) return BadRequest(new { message = "Debe proporcionar al menos un item para calificar" });
 var bloque = await _ctx.BloqueEvaluaciones.FindAsync(id);
 if(bloque==null) return NotFound(new{ message = "Bloque no encontrado" });
 var uidStr = User.FindFirstValue(ClaimTypes.NameIdentifier); int uid =0; if(!string.IsNullOrEmpty(uidStr)) uid = int.Parse(uidStr);

 // Si el usuario es Docente, verificar que esté asignado al curso correspondiente
 if (User.IsInRole("Docente"))
 {
 var asignado = await _ctx.CursoDocentes.AnyAsync(cd => cd.CursoId == bloque.CursoId && cd.DocenteId == uid && cd.Activo);
 if (!asignado)
 return StatusCode(StatusCodes.Status403Forbidden, new { message = "No autorizado: docente no asignado al curso" });
 }

 var matriculaIds = dto.Items.Select(i=>i.MatriculaId).Distinct().ToList();
 var existing = await _ctx.CalificacionBloques.Where(cb => cb.BloqueEvaluacionId==id && matriculaIds.Contains(cb.MatriculaId)).ToDictionaryAsync(cb => cb.MatriculaId);
 var toAdd = new List<CalificacionBloque>();
 var toUpdate = new List<CalificacionBloque>();
 var now = DateTime.UtcNow;
 foreach(var item in dto.Items){
 if(existing.TryGetValue(item.MatriculaId, out var cal)){
 cal.Nota = item.Nota;
 cal.Estado = item.Estado;
 cal.Observaciones = item.Observaciones;
 cal.FechaRegistro = now;
 cal.UsuarioRegistro = uid;
 toUpdate.Add(cal);
 // Update asistencias
 if(bloque.Tipo == "Asistencia" && item.Asistencias != null){
 var existingAsist = await _ctx.AsistenciaBloques.Where(ab => ab.CalificacionBloqueId == cal.Id).ToListAsync();
 _ctx.AsistenciaBloques.RemoveRange(existingAsist);
 var newAsist = (item.Asistencias ?? Enumerable.Empty<int>())
 .Select(fechaId => new AsistenciaBloque { CalificacionBloqueId = cal.Id, BloqueFechaId = fechaId, Asistio = true })
 .ToList();
 _ctx.AsistenciaBloques.AddRange(newAsist);
 }
 }else{
 var newCal = new CalificacionBloque{
 BloqueEvaluacionId = id,
 MatriculaId = item.MatriculaId,
 Nota = item.Nota,
 Estado = item.Estado,
 Observaciones = item.Observaciones,
 FechaRegistro = now,
 UsuarioRegistro = uid
 };
 toAdd.Add(newCal);
 // Add asistencias after save
 if(bloque.Tipo == "Asistencia" && item.Asistencias != null){
 // Will add after save
 }
 }
 }
 _ctx.CalificacionBloques.AddRange(toAdd);
 _ctx.CalificacionBloques.UpdateRange(toUpdate);
 await _ctx.SaveChangesAsync();
 // Now add asistencias for new
 foreach(var item in dto.Items.Where(i => !existing.ContainsKey(i.MatriculaId) && bloque.Tipo == "Asistencia" && i.Asistencias != null)){
 var cal = toAdd.First(c => c.MatriculaId == item.MatriculaId);
 var newAsist = item.Asistencias.Select(fechaId => new AsistenciaBloque { CalificacionBloqueId = cal.Id, BloqueFechaId = fechaId, Asistio = true }).ToList();
 _ctx.AsistenciaBloques.AddRange(newAsist);
 }
 await _ctx.SaveChangesAsync();
 return Ok(new{ message = "Calificaciones registradas", created = toAdd.Count + toUpdate.Count });
 }
 }
}
