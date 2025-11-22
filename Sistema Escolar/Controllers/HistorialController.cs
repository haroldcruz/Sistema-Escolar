using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaEscolar.Controllers
{
 // Docente, Coordinador, Administrador
 [Authorize(Roles = "Docente,Coordinador,Administrador")]
 public class HistorialController : Controller
 {
 public IActionResult Index()
 {
 return View();
 }
 }
}