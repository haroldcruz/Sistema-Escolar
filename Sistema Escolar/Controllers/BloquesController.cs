using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaEscolar.Controllers
{
 [Authorize(Roles = "Docente,Coordinador,Administrador")]
 public class BloquesController : Controller
 {
 [HttpGet]
 public IActionResult Index()
 {
 return View();
 }

 [HttpGet]
 public IActionResult Calificar(int id)
 {
 ViewBag.BloqueId = id;
 return View();
 }
 }
}
