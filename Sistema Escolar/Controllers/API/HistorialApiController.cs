using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Interfaces.Historial;
using System.Security.Claims;

namespace SistemaEscolar.Controllers.API
{
 // API para historial académico
 [ApiController]
 [Route("api/[controller]")]
 [Authorize(Policy = "Historial.Ver")]
 public class HistorialApiController : ControllerBase
 {
 private readonly IHistorialService _service;

 public HistorialApiController(IHistorialService service)
 {
 _service = service;
 }

 private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id :0;
 private bool IsStudentOnly => User.IsInRole("Estudiante") && !User.IsInRole("Docente") && !User.IsInRole("Coordinador") && !User.IsInRole("Administrador");

 // GET: api/historial/{estudianteId}
 [HttpGet("{estudianteId}")]
 public async Task<IActionResult> GetHistorial(int estudianteId)
 {
 if (IsStudentOnly && estudianteId != CurrentUserId) return Forbid();
 var historial = await _service.GetHistorialAsync(estudianteId);
 if (historial == null) return NotFound(new { message = "Estudiante no encontrado o sin historial" });
 return Ok(historial);
 }

 // GET: api/historial/agrupado/{estudianteId}
 [HttpGet("agrupado/{estudianteId}")]
 public async Task<IActionResult> GetHistorialAgrupado(int estudianteId)
 {
 if (IsStudentOnly && estudianteId != CurrentUserId) return Forbid();
 var historial = await _service.GetHistorialAgrupadoAsync(estudianteId);
 if (historial == null) return NotFound(new { message = "Estudiante no encontrado o sin historial" });
 return Ok(historial);
 }

 // GET: api/historial/filtrado/{estudianteId}
 [HttpGet("filtrado/{estudianteId}")]
 public async Task<IActionResult> GetHistorialFiltrado(int estudianteId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string[]? cursos)
 {
 if (IsStudentOnly && estudianteId != CurrentUserId) return Forbid();
 var historial = await _service.GetHistorialAgrupadoFiltradoAsync(estudianteId, from, to, cursos);
 if (historial == null) return NotFound(new { message = "Estudiante no encontrado o sin historial" });
 return Ok(historial);
 }

 // GET: api/historial/grafico/{estudianteId}
 [HttpGet("grafico/{estudianteId}")]
 public async Task<IActionResult> GetGrafico(int estudianteId)
 {
 if (IsStudentOnly && estudianteId != CurrentUserId) return Forbid();
 var agrupado = await _service.GetHistorialAgrupadoAsync(estudianteId);
 if (agrupado == null) return NotFound(new { message = "Estudiante no encontrado" });
 var serie = agrupado.Cuatrimestres.Select(c => new { cuatrimestre = c.Cuatrimestre, promedio = c.Promedio ??0m });
 return Ok(serie);
 }

 // GET: api/historial/buscar?term=texto
 [HttpGet("buscar")]
 public async Task<IActionResult> Buscar([FromQuery] string term)
 {
 // Estudiante solo puede buscarse a sí mismo
 if (IsStudentOnly)
 {
 var propio = await _service.GetHistorialAsync(CurrentUserId);
 return Ok(new[] { new { id = propio.EstudianteId, nombreCompleto = propio.NombreCompleto, identificacion = "", email = "" } });
 }
 var resultados = await _service.BuscarEstudiantesAsync(term);
 return Ok(resultados);
 }
 }
}
