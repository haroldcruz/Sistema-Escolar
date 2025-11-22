using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaEscolar.Controllers
{
 // Solo administrador
 [Authorize(Roles = "Administrador")]
 [Authorize(Policy = "Bitacora.Ver")]
 public class BitacoraController : Controller
 {
 public IActionResult Index()
 {
 return View();
 }
 }
}