using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SistemaEscolar.Controllers
{
 // Docente, Coordinador, Administrador
 [Authorize(Roles = "Docente,Coordinador,Administrador")]
 public class HistorialController : Controller
 {
 public IActionResult Index() { return View(); }
 }
 // NUEVO controlador separado para estudiante propio
 [Authorize(Roles="Estudiante")]
 [Route("MiHistorial")] // /MiHistorial
 public class MiHistorialController : Controller
 {
 [HttpGet("")]
 public IActionResult Index(){ return View(); }
 }
}