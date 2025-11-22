using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Interfaces.Bitacora;

namespace SistemaEscolar.Controllers.API
{
 // API para consulta de bitácora
 [ApiController]
 [Route("api/[controller]")]
 [Authorize(Policy = "Bitacora.Ver")] // solo con permiso explícito
 public class BitacoraApiController : ControllerBase
 {
 private readonly IBitacoraService _service;

 public BitacoraApiController(IBitacoraService service)
 {
 _service = service;
 }

 // GET: api/bitacora
 [HttpGet]
 public async Task<IActionResult> GetAll()
 {
 var registros = await _service.GetAllAsync();
 return Ok(registros);
 }

 // GET: api/bitacora/paged?from=2024-01-01&to=2024-12-31&usuario=Juan&accion=Login&modulo=Usuarios&page=1&pageSize=20
 [HttpGet("paged")]
 public async Task<IActionResult> GetPaged([FromQuery] int page =1, [FromQuery] int pageSize =20, [FromQuery] string? usuario = null, [FromQuery] string? modulo = null, [FromQuery] string? accion = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
 {
 if (pageSize >200) pageSize =200; // límite
 var registros = await _service.GetPagedAsync(page, pageSize, usuario, modulo, accion);
 if (from.HasValue) registros = registros.Where(r => DateTime.Parse(r.Fecha) >= from.Value);
 if (to.HasValue) registros = registros.Where(r => DateTime.Parse(r.Fecha) <= to.Value);
 return Ok(registros);
 }
 }
}
