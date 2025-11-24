using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/matriculas")]
 [Authorize(Roles = "Administrador")] // solo administradores pueden matricular
 public class MatriculasApiController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 public MatriculasApiController(ApplicationDbContext ctx){ _ctx = ctx; }

 public class MatriculaCreateDTO { public int EstudianteId { get; set; } public int CursoId { get; set; } public int CuatrimestreId { get; set; } public int CursoOfertaId { get; set; } }

 [HttpPost]
 public async Task<IActionResult> Create([FromBody] MatriculaCreateDTO dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 // Verificar existencia
 var estudiante = await _ctx.Usuarios.FindAsync(dto.EstudianteId);
 if (estudiante == null) return NotFound(new { message = "Estudiante no encontrado" });
 // Determinar oferta a usar
 int ofertaId = dto.CursoOfertaId;
 if (ofertaId ==0)
 {
 // intentar encontrar oferta por curso+cuatrimestre
 var oferta = await _ctx.CursoOfertas.FirstOrDefaultAsync(co => co.CursoId == dto.CursoId && co.CuatrimestreId == dto.CuatrimestreId);
 if (oferta != null) ofertaId = oferta.Id;
 else
 {
 // crear oferta por defecto (Grupo A)
 var nueva = new Models.Academico.CursoOferta { CursoId = dto.CursoId, CuatrimestreId = dto.CuatrimestreId, NombreGrupo = "A", FechaCreacion = DateTime.UtcNow };
 _ctx.CursoOfertas.Add(nueva);
 await _ctx.SaveChangesAsync();
 ofertaId = nueva.Id;
 }
 }
 // Evitar duplicados por oferta
 var exists = await _ctx.Matriculas.AnyAsync(m => m.EstudianteId == dto.EstudianteId && m.CursoOfertaId == ofertaId);
 if (exists) return Conflict(new { message = "El estudiante ya está matriculado en esa oferta/grupo" });
 // Obtener datos para respuesta
 var ofertaObj = await _ctx.CursoOfertas.Include(co=>co.Curso).Include(co=>co.Cuatrimestre).FirstOrDefaultAsync(co=>co.Id==ofertaId);
 if (ofertaObj == null) return BadRequest(new { message = "Oferta no encontrada" });
 var m = new Models.Academico.Matricula{ EstudianteId = dto.EstudianteId, CursoOfertaId = ofertaId, CursoId = ofertaObj.CursoId, CuatrimestreId = ofertaObj.CuatrimestreId, FechaMatricula = DateTime.UtcNow };
 _ctx.Matriculas.Add(m);
 await _ctx.SaveChangesAsync();
 // Devolver detalles de la nueva matrícula para que cliente actualice la UI
 return Ok(new { message = "Matriculado", matricula = new { id = m.Id, curso = ofertaObj.Curso?.Nombre, cuatrimestre = ofertaObj.Cuatrimestre?.Nombre, ofertaId = ofertaId, grupo = ofertaObj.NombreGrupo } });
 }
 }
}
