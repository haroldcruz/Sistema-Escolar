using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaEscolar.Controllers
{
 // Administrador y Coordinador
 [Authorize(Roles = "Administrador,Coordinador")]
 public class CursosController : Controller
 {
 public IActionResult Index()
 {
 return View();
 }
 }
}