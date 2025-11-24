using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Evaluaciones;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using SistemaEscolar.Interfaces.Bitacora;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/evaluaciones")]
 [Authorize]
 public class EvaluacionesApiController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 private readonly IBitacoraService _bitacora;
 public EvaluacionesApiController(ApplicationDbContext ctx, IBitacoraService bitacora) { _ctx = ctx; _bitacora = bitacora; }

 private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

 // GET: api/evaluaciones/mis-cursos
 [HttpGet("mis-cursos")]
 public async Task<IActionResult> GetMisCursos()
 {
 var uid = CurrentUserId();
 if (uid ==0) return Unauthorized();

 var cursos = await _ctx.Cursos
 .Include(c => c.Cuatrimestre)
 .Include(c => c.CursoDocentes).ThenInclude(cd => cd.Docente)
 .Where(c => c.CursoDocentes.Any(cd => cd.DocenteId == uid && cd.Activo))
 .Select(c => new {
 Id = c.Id,
 Codigo = c.Codigo,
 Nombre = c.Nombre,
 Creditos = c.Creditos,
 Cuatrimestre = c.Cuatrimestre != null ? c.Cuatrimestre.Nombre : string.Empty,
 Docentes = c.CursoDocentes.Select(cd => (cd.Docente.Nombre + " " + cd.Docente.Apellidos).Trim()).ToList()
 })
 .ToListAsync();

 return Ok(cursos);
 }

 // GET: api/evaluaciones/cursos/5/estudiantes
 [HttpGet("cursos/{cursoId:int}/estudiantes")]
 public async Task<IActionResult> GetEstudiantesPorCurso(int cursoId)
 {
 var uid = CurrentUserId();
 if (uid ==0) return Unauthorized();

 // verificar que el docente actual esté asignado al curso o sea admin
 var esDocenteAsignado = await _ctx.CursoDocentes.AnyAsync(cd => cd.CursoId == cursoId && cd.DocenteId == uid && cd.Activo);
 var esAdmin = User.IsInRole("Administrador");
 if (!esDocenteAsignado && !esAdmin) return Forbid();

 var estudiantes = await _ctx.Matriculas
 .Include(m => m.Estudiante)
 .Include(m => m.Cuatrimestre)
 .Where(m => m.CursoId == cursoId)
 .Select(m => new {
 MatriculaId = m.Id,
 EstudianteId = m.EstudianteId,
 NombreCompleto = (m.Estudiante.Nombre + " " + m.Estudiante.Apellidos).Trim(),
 Identificacion = m.Estudiante.Identificacion,
 Cuatrimestre = m.Cuatrimestre != null ? m.Cuatrimestre.Nombre : string.Empty,
 FechaMatricula = m.FechaMatricula
 })
 .ToListAsync();

 return Ok(estudiantes);
 }

 // GET: api/evaluaciones/estudiante/{estudianteId}/matriculas
 [HttpGet("estudiante/{estudianteId:int}/matriculas")]
 public async Task<IActionResult> GetMatriculasDelEstudianteEnMisCursos(int estudianteId)
 {
 var uid = CurrentUserId();
 if (uid ==0) return Unauthorized();

 // If caller is a student, only allow access to their own id
 if (User.IsInRole("Estudiante") && estudianteId != uid) return Forbid();

 // Obtener matriculas del estudiante donde el curso está asignado al docente
 var query = _ctx.Matriculas
 .Include(m => m.Curso)
 .Include(m => m.Cuatrimestre)
 .Include(m => m.Estudiante)
 .Where(m => m.EstudianteId == estudianteId && _ctx.CursoDocentes.Any(cd => cd.CursoId == m.CursoId && cd.DocenteId == uid && cd.Activo));

 var lista = await query.Select(m => new {
 MatriculaId = m.Id,
 CursoId = m.CursoId,
 CursoCodigo = m.Curso.Codigo,
 CursoNombre = m.Curso.Nombre,
 CuatrimestreId = m.CuatrimestreId,
 Cuatrimestre = m.Cuatrimestre != null ? m.Cuatrimestre.Nombre : string.Empty,
 NombreCompleto = (m.Estudiante.Nombre + " " + m.Estudiante.Apellidos).Trim()
 }).ToListAsync();

 return Ok(lista);
 }

 // POST: api/evaluaciones
 [HttpPost]
 [Authorize(Policy = "Evaluaciones.Crear")]
 public async Task<IActionResult> Crear([FromBody] EvaluacionCreateDTO dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);

 var uid = CurrentUserId();
 if (uid ==0) return Unauthorized();

 // validar matricula
 var matricula = await _ctx.Matriculas.Include(m => m.Curso).FirstOrDefaultAsync(m => m.Id == dto.MatriculaId);
 if (matricula == null) return NotFound(new { message = "Matrícula no encontrada" });

 // verificar que el docente actual esté asignado al curso correspondiente
 var cursoId = matricula.CursoId;
 var esDocenteAsignado = await _ctx.CursoDocentes.AnyAsync(cd => cd.CursoId == cursoId && cd.DocenteId == uid && cd.Activo);
 var esAdmin = User.IsInRole("Administrador");
 if (!esDocenteAsignado && !esAdmin) return Forbid();

 // Evitar duplicados: ya existe evaluación para la misma matrícula?
 var existe = await _ctx.Evaluaciones.AnyAsync(e => e.MatriculaId == dto.MatriculaId);
 if (existe) return Conflict(new { message = "Ya existe una evaluación para esta matrícula" });

 var evaluacion = new Models.Academico.Evaluacion
 {
 MatriculaId = dto.MatriculaId,
 Nota = dto.Nota,
 Observaciones = dto.Observaciones,
 Participacion = dto.Participacion,
 Estado = dto.Estado,
 FechaRegistro = DateTime.UtcNow,
 UsuarioRegistro = uid
 };

 _ctx.Evaluaciones.Add(evaluacion);
 await _ctx.SaveChangesAsync();

 // Registrar en bitácora
 try
 {
 var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
 await _bitacora.RegistrarAsync(uid, $"Crear evaluación Matricula:{dto.MatriculaId} Nota:{dto.Nota}", "Evaluaciones", ip);
 }
 catch { /* no bloquear por error de bitácora */ }

 return CreatedAtAction(nameof(GetEstudiantesPorCurso), new { cursoId = cursoId }, new { message = "Evaluación creada", evaluacionId = evaluacion.Id });
 }
 }
}
