using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaEscolar.Controllers
{
 [Authorize(Roles = "Docente,Coordinador,Administrador")]
 public class EstadisticasController : Controller
 {
 public IActionResult Index()
 {
 return View();
 }
 }
}