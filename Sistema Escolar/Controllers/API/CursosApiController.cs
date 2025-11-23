using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Interfaces.Cursos;
using SistemaEscolar.DTOs.Cursos;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/cursos")] // Ruta fija para coincidir con fetch('/api/cursos')
 [Produces("application/json")]
 [Authorize(Policy = "Cursos.Ver")]
 public class CursosApiController : ControllerBase
 {
 private readonly ICursoService _cursos;
 public CursosApiController(ICursoService cursos){ _cursos = cursos; }

 private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
 private string Ip() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

 // GET api/cursos
 [HttpGet]
 public async Task<IActionResult> GetAll()
 {
 var lista = await _cursos.GetAllAsync();
 return Ok(lista);
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
 var ok = await _cursos.CreateAsync(dto, CurrentUserId(), Ip());
 if (!ok) return BadRequest(new { message = "No se pudo crear (código duplicado u otros)." });
 return Ok(new { message = "Curso creado" });
 }

 // PUT api/cursos/{id}
 [HttpPut("{id:int}")]
 [Authorize(Policy = "Cursos.Editar")]
 public async Task<IActionResult> Update(int id, [FromBody] CursoUpdateDTO dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var ok = await _cursos.UpdateAsync(id, dto, CurrentUserId(), Ip());
 if (!ok) return NotFound(new { message = "No se pudo actualizar" });
 return Ok(new { message = "Curso actualizado" });
 }

 // DELETE api/cursos/{id}
 [HttpDelete("{id:int}")]
 [Authorize(Policy = "Cursos.Eliminar")]
 public async Task<IActionResult> Delete(int id)
 {
 var ok = await _cursos.DeleteAsync(id, CurrentUserId(), Ip());
 if (!ok) return NotFound(new { message = "No se pudo eliminar" });
 return Ok(new { message = "Curso eliminado" });
 }
 }
}
