using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Cursos;
using SistemaEscolar.Interfaces.Cursos;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/cursos")]
 [Produces("application/json")]
 public class CursosApiController : ControllerBase
 {
 private readonly ICursoService _cursos;
 private readonly ApplicationDbContext _ctx;
 public CursosApiController(ICursoService cursos, ApplicationDbContext ctx) { _cursos = cursos; _ctx = ctx; }

 private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
 private string Ip() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

 // GET api/cursos
 [HttpGet]
 [Authorize(Policy = "Cursos.Ver")]
 public async Task<IActionResult> GetAll([FromQuery] int? cuatrimestreId)
 {
 var q = _ctx.Cursos.AsNoTracking().Include(c => c.Cuatrimestre).AsQueryable();
 if (cuatrimestreId.HasValue)
 q = q.Where(c => c.CuatrimestreId == cuatrimestreId.Value);
 var list = await q.OrderBy(c => c.Nombre).ToListAsync();
 var result = list.Select(c => new { id = c.Id, codigo = c.Codigo, nombre = c.Nombre });
 return Ok(result);
 }

 // GET api/cursos/{id}
 [HttpGet("{id:int}")]
 public async Task<IActionResult> Get(int id)
 {
 var curso = await _cursos.GetByIdAsync(id);
 if (curso == null) return NotFound();
 return Ok(curso);
 }

 // POST api/cursos
 [HttpPost]
 [Authorize(Policy = "Cursos.Crear")]
 public async Task<IActionResult> Create([FromBody] CursoCreateDTO dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
 var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
 var ok = await _cursos.CreateAsync(dto, uid, ip);
 if (!ok) return BadRequest(new { message = "No se pudo crear (quizá código duplicado)" });
 return Ok(new { message = "Curso creado" });
 }

 // PUT api/cursos/{id}
 [HttpPut("{id:int}")]
 [Authorize(Policy = "Cursos.Editar")]
 public async Task<IActionResult> Update(int id, [FromBody] CursoUpdateDTO dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var (ok, error) = await _cursos.UpdateAsync(id, dto, CurrentUserId(), Ip());
 if (!ok)
 {
 // If specific error like code-change blocked or duplicate, return400 with message
 return BadRequest(new { message = error ?? "No se pudo actualizar" });
 }
 return Ok(new { message = "Curso actualizado" });
 }

 // DELETE api/cursos/{id}
 [HttpDelete("{id:int}")]
 [Authorize(Policy = "Cursos.Eliminar")]
 public async Task<IActionResult> Delete(int id)
 {
 // Validar existencia
 var existe = await _ctx.Cursos.AnyAsync(c => c.Id == id);
 if (!existe) return NotFound(new { message = "No encontrado" });
 // Regla: si tiene matriculas ->409 con mensaje escueto
 var tieneMatriculas = await _ctx.Matriculas.AnyAsync(m => m.CursoId == id && m.Activo);
 if (tieneMatriculas) return StatusCode(409, new { message = "Estudiantes matriculados" });
 // Proceder eliminación
 var ok = await _cursos.DeleteAsync(id, CurrentUserId(), Ip());
 if (!ok) return BadRequest(new { message = "No se pudo eliminar" });
 return Ok(new { message = "Eliminado" });
 }

 // Nuevo endpoint para obtener ofertas por curso
 [HttpGet("{cursoId}/ofertas")]
 public async Task<IActionResult> GetOfertas(int cursoId){
 var ofertas = await _ctx.CursoOfertas.Where(co=> co.CursoId==cursoId).Select(co => new { id = co.Id, nombre = co.NombreGrupo, cuatrimestreId = co.CuatrimestreId, cursoId = co.CursoId }).ToListAsync();
 return Ok(ofertas);
 }
 }
}
