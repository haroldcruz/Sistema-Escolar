using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Interfaces.Historial;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;

namespace SistemaEscolar.Controllers.API
{
 [Route("api/historial")] // Ruta fija para que sea /api/historial
 [ApiController]
 public class HistorialApiController : ControllerBase
 {
 private readonly IHistorialService _historial;

 public HistorialApiController(IHistorialService historial){ _historial = historial; }

 // Búsqueda para personal autorizado
 [HttpGet("buscar")]
 [Authorize(Roles="Docente,Coordinador,Administrador")]
 public async Task<IActionResult> Buscar([FromQuery] string term){ var res = await _historial.BuscarEstudiantesAsync(term); return Ok(res.Select(x=> new { id=x.Id, nombreCompleto=x.NombreCompleto, identificacion=x.Identificacion, email=x.Email })); }

 // Historial por estudiante (staff)
 [HttpGet("{id:int}/agrupado")]
 [Authorize(Roles="Docente,Coordinador,Administrador")]
 public async Task<IActionResult> Agrupado(int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string[]? cursos){ var dto = await _historial.GetHistorialAgrupadoFiltradoAsync(id, from, to, cursos); return Ok(dto); }

 // NUEVO: historial propio (estudiante)
 [HttpGet("mio")]
 [Authorize(Roles="Estudiante")]
 public async Task<IActionResult> MiHistorial([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string[]? cursos)
 {
 var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
 if (string.IsNullOrEmpty(idStr)) return Unauthorized();
 if(!int.TryParse(idStr, out var id)) return Unauthorized();
 var dto = await _historial.GetHistorialAgrupadoFiltradoAsync(id, from, to, cursos);
 return Ok(dto);
 }
 }
}
