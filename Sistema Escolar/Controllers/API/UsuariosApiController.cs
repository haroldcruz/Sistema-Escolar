using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.DTOs.Usuarios;
using SistemaEscolar.Interfaces.Usuarios;

namespace SistemaEscolar.Controllers.API
{
 // API para CRUD de usuarios
 [ApiController]
 [Route("api/[controller]")]
 [Authorize(Policy = "Usuarios.Gestion")] // protección por permiso granular
 public class UsuariosApiController : ControllerBase
 {
 private readonly IUsuarioService _service;

 public UsuariosApiController(IUsuarioService service)
 {
 _service = service;
 }

 // GET: api/usuarios
 [HttpGet]
 public async Task<IActionResult> GetAll()
 {
 var usuarios = await _service.GetAllAsync();
 return Ok(usuarios);
 }

 // GET: api/usuarios/5
 [HttpGet("{id}")]
 public async Task<IActionResult> GetById(int id)
 {
 var usuario = await _service.GetByIdAsync(id);

 if (usuario == null)
 return NotFound(new { message = "Usuario no encontrado" });

 return Ok(usuario);
 }

 // POST: api/usuarios
 [HttpPost]
 public async Task<IActionResult> Create([FromBody] UsuarioCreateDTO dto)
 {
 var ok = await _service.CreateAsync(dto);

 if (!ok)
 return BadRequest(new { message = "No se pudo crear el usuario" });

 return Ok(new { message = "Usuario creado" });
 }

 // PUT: api/usuarios/5
 [HttpPut("{id}")]
 public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdateDTO dto)
 {
 var ok = await _service.UpdateAsync(id, dto);

 if (!ok)
 return NotFound(new { message = "Usuario no encontrado" });

 return Ok(new { message = "Usuario actualizado" });
 }

 // DELETE: api/usuarios/5
 [HttpDelete("{id}")]
 public async Task<IActionResult> Delete(int id)
 {
 var ok = await _service.DeleteAsync(id);

 if (!ok)
 return NotFound(new { message = "Usuario no encontrado" });

 return Ok(new { message = "Usuario eliminado" });
 }
 }
}
