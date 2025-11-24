using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Interfaces.Historial;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/historial")]
 [Authorize]
 public class HistorialApiController : ControllerBase
 {
 private readonly IHistorialService _historial;
 public HistorialApiController(IHistorialService historial) { _historial = historial; }

 private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

 // GET api/historial/mio
 [HttpGet("mio")]
 [Authorize(Roles = "Estudiante")]
 public async Task<IActionResult> GetMiHistorial()
 {
 var uid = CurrentUserId();
 if (uid ==0) return Unauthorized();
 var dto = await _historial.GetHistorialAgrupadoAsync(uid);
 return Ok(dto);
 }

 // GET api/historial/mio/filtrado?from=2025-01-01&to=2025-12-31
 [HttpGet("mio/filtrado")]
 [Authorize(Roles = "Estudiante")]
 public async Task<IActionResult> GetMiHistorialFiltrado([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? cursos)
 {
 var uid = CurrentUserId();
 if (uid ==0) return Unauthorized();
 IEnumerable<string>? cursosList = null;
 if (!string.IsNullOrWhiteSpace(cursos)) cursosList = cursos.Split(',');
 var dto = await _historial.GetHistorialAgrupadoFiltradoAsync(uid, from, to, cursosList);
 return Ok(dto);
 }
 }
}
