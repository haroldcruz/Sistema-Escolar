using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/catalogo/cuatrimestres")]
 [Authorize] // permitir cualquier usuario autenticado (Docente, Administrador, etc.)
 public class CatalogoCuatrimestresApiController : ControllerBase
 {
 private readonly ApplicationDbContext _ctx;
 public CatalogoCuatrimestresApiController(ApplicationDbContext ctx){ _ctx = ctx; }
 [HttpGet]
 public async Task<IActionResult> Get(){
 var list = await _ctx.Cuatrimestres.AsNoTracking().OrderBy(c=>c.Nombre).Select(c=> new{ c.Id, c.Nombre}).ToListAsync();
 return Ok(list);
 }
 }
}
