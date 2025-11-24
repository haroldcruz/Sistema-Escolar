using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Interfaces.Historial;
using System.Threading.Tasks;
using System.Security.Claims;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/estudiantes")]
 [Authorize(Roles = "Docente,Coordinador,Administrador")]
 public class EstudiantesApiController : ControllerBase
 {
 private readonly IHistorialService _historial;
 public EstudiantesApiController(IHistorialService historial) { _historial = historial; }

 // GET api/estudiantes/buscar?term=...
 [HttpGet("buscar")]
 public async Task<IActionResult> Buscar([FromQuery] string term)
 {
 var lista = await _historial.BuscarEstudiantesAsync(term);
 return Ok(lista);
 }
 }
}
