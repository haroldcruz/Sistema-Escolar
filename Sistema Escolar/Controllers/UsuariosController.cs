using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaEscolar.Controllers
{
 // Vista de usuarios protegida por permiso granular
 [Authorize(Policy = "Usuarios.Gestion")]
 public class UsuariosController : Controller
 {
 public IActionResult Index()
 {
 return View();
 }
 }
}