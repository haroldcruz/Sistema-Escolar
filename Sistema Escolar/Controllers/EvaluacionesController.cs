using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Evaluaciones;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using System;

namespace SistemaEscolar.Controllers
{
 [Authorize(Roles = "Docente,Coordinador,Administrador")]
 public class EvaluacionesController : Controller
 {
 private readonly ApplicationDbContext _ctx;
 public EvaluacionesController(ApplicationDbContext ctx) { _ctx = ctx; }

 public IActionResult Index()
 {
 // Página para buscar estudiantes y registrar evaluación
 return View();
 }

 [HttpGet]
 public async Task<IActionResult> BuscarEstudiantes(string q)
 {
 if (string.IsNullOrWhiteSpace(q)) return Json(new object[0]);
 var list = await _ctx.Usuarios
 .Where(u => u.Nombre.Contains(q) || u.Apellidos.Contains(q) || u.Identificacion.Contains(q))
 .OrderBy(u => u.Nombre)
 .Take(20)
 .Select(u => new { u.Id, Nombre = u.Nombre + " " + u.Apellidos, u.Identificacion, u.Email })
 .ToListAsync();
 return Json(list);
 }

 [HttpGet]
 public async Task<IActionResult> MatriculasDeEstudiante(int estudianteId)
 {
 // devolver matriculas activas del estudiante para que docente seleccione curso/cuatrimestre
 var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

 var query = _ctx.Matriculas.Include(m => m.Curso).Include(m => m.Cuatrimestre).Where(m => m.EstudianteId == estudianteId);

 // Si el usuario es Docente, restringir a matriculas de cursos donde el docente está asignado
 if (User.IsInRole("Docente") && uid >0)
 {
 query = query.Where(m => _ctx.CursoDocentes.Any(cd => cd.CursoId == m.CursoId && cd.DocenteId == uid));
 }

 var list = await query
 .Select(m => new { m.Id, CursoId = m.CursoId, Curso = m.Curso.Nombre, CuatrimestreId = m.CuatrimestreId, Cuatrimestre = m.Cuatrimestre.Nombre })
 .ToListAsync();
 return Json(list);
 }

 [HttpPost]
 public async Task<IActionResult> Guardar([FromBody] EvaluacionCreateDTO dto)
 {
 if (!ModelState.IsValid) return BadRequest(new { message = "Datos inválidos" });
 // validar existencia de matricula
 var matricula = await _ctx.Matriculas.Include(m => m.Curso).Include(m => m.Cuatrimestre).FirstOrDefaultAsync(m => m.Id == dto.MatriculaId);
 if (matricula == null) return NotFound(new { message = "Matrícula no encontrada" });

 var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
 // Si es Docente, validar que el docente está asignado al curso de la matrícula
 if (User.IsInRole("Docente") && uid >0)
 {
 var assigned = await _ctx.CursoDocentes.AnyAsync(cd => cd.CursoId == matricula.CursoId && cd.DocenteId == uid);
 if (!assigned) return Forbid();
 }

 // evitar duplicados: si ya existe evaluación para misma matrícula y mismo cuatrimestre
 var exists = await _ctx.Evaluaciones
 .Include(e => e.Matricula)
 .AnyAsync(e => e.MatriculaId == dto.MatriculaId && e.Matricula!.CuatrimestreId == matricula.CuatrimestreId);
 if (exists) return BadRequest(new { message = "Ya existe evaluación para esta matrícula y cuatrimestre" });

 var eval = new Models.Academico.Evaluacion
 {
 MatriculaId = dto.MatriculaId,
 Nota = dto.Nota,
 Observaciones = dto.Observaciones,
 Participacion = dto.Participacion,
 Estado = dto.Estado,
 FechaRegistro = DateTime.UtcNow
 };
 _ctx.Evaluaciones.Add(eval);
 await _ctx.SaveChangesAsync();
 return Ok(new { message = "Evaluación registrada", evaluacion = new { eval.Id, eval.Nota, eval.Estado, eval.Participacion, eval.Observaciones, Fecha = eval.FechaRegistro } });
 }
 }
}
