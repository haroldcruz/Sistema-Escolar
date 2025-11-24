using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.DTOs.Usuarios;
using SistemaEscolar.Interfaces.Usuarios;
using System.Threading.Tasks;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/usuarios")]
 [Authorize(Policy = "Usuarios.Gestion")]
 public class UsuariosApiController : ControllerBase
 {
 private readonly IUsuarioService _service;

 public UsuariosApiController(IUsuarioService service)
 {
 _service = service;
 }

 [HttpGet]
 public async Task<IActionResult> GetAll()
 {
 var usuarios = await _service.GetAllAsync();
 return Ok(usuarios);
 }

 [HttpGet("{id}")]
 public async Task<IActionResult> GetById(int id)
 {
 var usuario = await _service.GetByIdAsync(id);
 if (usuario == null) return NotFound(new { message = "Usuario no encontrado" });
 return Ok(usuario);
 }

 [HttpPost]
 public async Task<IActionResult> Create([FromBody] UsuarioCreateDTO dto)
 {
 var (ok, error) = await _service.CreateAsync(dto);
 if (!ok) return BadRequest(new { message = error ?? "No se pudo crear el usuario" });
 return Ok(new { message = "Usuario creado" });
 }

 [HttpPut("{id}")]
 public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdateDTO dto)
 {
 var (ok, error) = await _service.UpdateAsync(id, dto);
 if (!ok) return BadRequest(new { message = error ?? "No se pudo actualizar" });
 return Ok(new { message = "Usuario actualizado" });
 }

 [HttpDelete("{id}")]
 public async Task<IActionResult> Delete(int id)
 {
 var (ok, error) = await _service.DeleteAsync(id);
 if (!ok) return BadRequest(new { message = error ?? "No se pudo eliminar" });
 return Ok(new { message = "Usuario eliminado" });
 }
 }
}
