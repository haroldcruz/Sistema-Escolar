using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.DTOs.Cursos;
using SistemaEscolar.Interfaces.Cursos;

namespace SistemaEscolar.Controllers.API
{
 // API para gestión de cursos
 [ApiController]
 [Route("api/[controller]")]
 [Authorize(Roles = "Administrador,Coordinador")]
 public class CursosApiController : ControllerBase
 {
 private readonly ICursoService _service;

 public CursosApiController(ICursoService service)
 {
 _service = service;
 }

 // GET: api/cursos
 [HttpGet]
 public async Task<IActionResult> GetAll()
 {
 var cursos = await _service.GetAllAsync();
 return Ok(cursos);
 }

 // GET: api/cursos/{id}
 [HttpGet("{id}")]
 public async Task<IActionResult> GetById(int id)
 {
 var curso = await _service.GetByIdAsync(id);

 if (curso == null)
 return NotFound(new { message = "Curso no encontrado" });

 return Ok(curso);
 }

 // POST: api/cursos
 [HttpPost]
 public async Task<IActionResult> Create([FromBody] CursoCreateDTO dto)
 {
 var ok = await _service.CreateAsync(dto);

 if (!ok)
 return BadRequest(new { message = "No se pudo crear el curso" });

 return Ok(new { message = "Curso creado" });
 }

 // PUT: api/cursos/{id}
 [HttpPut("{id}")]
 public async Task<IActionResult> Update(int id, [FromBody] CursoUpdateDTO dto)
 {
 var ok = await _service.UpdateAsync(id, dto);

 if (!ok)
 return NotFound(new { message = "Curso no encontrado" });

 return Ok(new { message = "Curso actualizado" });
 }

 // DELETE: api/cursos/{id}
 [HttpDelete("{id}")]
 public async Task<IActionResult> Delete(int id)
 {
 var ok = await _service.DeleteAsync(id);

 if (!ok)
 return NotFound(new { message = "Curso no encontrado" });

 return Ok(new { message = "Curso eliminado" });
 }

 // POST: api/cursos/asignar-docente
 [HttpPost("asignar-docente")]
 public async Task<IActionResult> AsignarDocente([FromBody] CursoDocenteDTO dto)
 {
 var ok = await _service.AsignarDocenteAsync(dto);

 if (!ok)
 return BadRequest(new { message = "No se pudo asignar el docente" });

 return Ok(new { message = "Docente asignado" });
 }
 }
}
