using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/cursos/{cursoId:int}/ofertas")]
 public class CursoOfertasApiController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 public CursoOfertasApiController(ApplicationDbContext ctx) { _ctx = ctx; }

 // Crear oferta (Administrador)
 [HttpPost]
 [Authorize(Roles = "Administrador")]
 public async Task<IActionResult> Create(int cursoId, [FromBody] OfertaCreateDTO dto)
 {
 if (!await _ctx.Cursos.AnyAsync(c => c.Id == cursoId)) return NotFound(new { message = "Curso no encontrado" });
 if (dto == null) return BadRequest();
 var existe = await _ctx.CursoOfertas.AnyAsync(co => co.CursoId == cursoId && co.CuatrimestreId == dto.CuatrimestreId && co.NombreGrupo == dto.NombreGrupo);
 if (existe) return Conflict(new { message = "Oferta ya existe para ese curso/cuatrimestre/grupo" });
 var oferta = new Models.Academico.CursoOferta { CursoId = cursoId, CuatrimestreId = dto.CuatrimestreId, NombreGrupo = dto.NombreGrupo ?? "A", Capacidad = dto.Capacidad };
 _ctx.CursoOfertas.Add(oferta);
 await _ctx.SaveChangesAsync();
 return Ok(new { message = "Oferta creada", id = oferta.Id });
 }

 // Obtener detalles de una oferta
 [HttpGet("{ofertaId:int}")]
 [Authorize]
 public async Task<IActionResult> Get(int cursoId, int ofertaId)
 {
 var oferta = await _ctx.CursoOfertas
 .Include(o => o.Curso)
 .Include(o => o.Cuatrimestre)
 .Include(o => o.CursoOfertaDocentes).ThenInclude(cd => cd.Docente)
 .Include(o => o.Matriculas)
 .FirstOrDefaultAsync(o => o.Id == ofertaId && o.CursoId == cursoId);
 if (oferta == null) return NotFound(new { message = "Oferta no encontrada" });
 var docentes = oferta.CursoOfertaDocentes.Select(d => new { docenteId = d.DocenteId, nombre = (d.Docente != null ? (d.Docente.Nombre + " " + d.Docente.Apellidos).Trim() : string.Empty) }).ToList();
 var matriculas = oferta.Matriculas.Select(m => new { id = m.Id, estudianteId = m.EstudianteId }).ToList();
 return Ok(new { id = oferta.Id, grupo = oferta.NombreGrupo, curso = oferta.Curso?.Nombre, cuatrimestre = oferta.Cuatrimestre?.Nombre, capacidad = oferta.Capacidad, docentes, matriculasCount = matriculas.Count });
 }

 // Asignar docente a oferta
 [HttpPost("{ofertaId:int}/docentes/{docenteId:int}")]
 [Authorize(Policy = "Cursos.AsignarDocente")]
 public async Task<IActionResult> AsignarDocente(int cursoId, int ofertaId, int docenteId)
 {
 var oferta = await _ctx.CursoOfertas.FindAsync(ofertaId);
 if (oferta == null || oferta.CursoId != cursoId) return NotFound(new { message = "Oferta no encontrada" });
 if (!await _ctx.Usuarios.AnyAsync(u => u.Id == docenteId)) return NotFound(new { message = "Docente no encontrado" });
 var exists = await _ctx.CursoOfertaDocentes.AnyAsync(cd => cd.CursoOfertaId == ofertaId && cd.DocenteId == docenteId);
 if (exists) return Conflict(new { message = "Docente ya asignado a la oferta" });
 _ctx.CursoOfertaDocentes.Add(new Models.Academico.CursoOfertaDocente { CursoOfertaId = ofertaId, DocenteId = docenteId });
 await _ctx.SaveChangesAsync();
 return Ok(new { message = "Docente asignado" });
 }

 // Quitar docente de oferta
 [HttpDelete("{ofertaId:int}/docentes/{docenteId:int}")]
 [Authorize(Policy = "Cursos.AsignarDocente")]
 public async Task<IActionResult> QuitarDocente(int cursoId, int ofertaId, int docenteId)
 {
 var rel = await _ctx.CursoOfertaDocentes.FirstOrDefaultAsync(cd => cd.CursoOfertaId == ofertaId && cd.DocenteId == docenteId);
 if (rel == null) return NotFound(new { message = "Relación no encontrada" });
 _ctx.CursoOfertaDocentes.Remove(rel);
 await _ctx.SaveChangesAsync();
 return Ok(new { message = "Docente quitado" });
 }

 public class OfertaCreateDTO { public int CuatrimestreId { get; set; } public string NombreGrupo { get; set; } = "A"; public int? Capacidad { get; set; } }
 }
}
