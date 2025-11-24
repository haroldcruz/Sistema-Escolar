using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using SistemaEscolar.Helpers;
using System.Linq;
using SistemaEscolar.DTOs.Usuarios;

namespace SistemaEscolar.Controllers
{
 [Authorize]
 [Route("Usuarios")]
 public class UsuariosController : Controller
 {
 private readonly ApplicationDbContext _ctx;
 public UsuariosController(ApplicationDbContext ctx) { _ctx = ctx; }

 private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
 private bool IsAdmin() => User.IsInRole("Administrador");

 // GET: /Usuarios/Editar/{id}
 [HttpGet("Editar/{id}")]
 public IActionResult Editar(int id)
 {
 var uid = CurrentUserId();
 if (uid ==0) return Unauthorized();
 // Si no es admin, sólo puede editar su propio perfil
 if (!IsAdmin() && id != uid) return RedirectToAction("AccessDenied", "Auth");

 var usuario = _ctx.Usuarios.Find(id);
 if (usuario == null) return NotFound();

 var model = new UsuarioUpdateDTO
 {
 Id = usuario.Id,
 Nombre = usuario.Nombre,
 Apellidos = usuario.Apellidos,
 Email = usuario.Email,
 Identificacion = usuario.Identificacion
 };
 return View(model);
 }

 // POST: /Usuarios/Editar/{id}
 [HttpPost("Editar/{id}")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> EditarPost(int id, UsuarioUpdateDTO model)
 {
 var uid = CurrentUserId();
 if (uid ==0) return Unauthorized();
 if (!IsAdmin() && id != uid) return RedirectToAction("AccessDenied", "Auth");
 if (id != model.Id) return BadRequest();
 if (!ModelState.IsValid) return View("Editar", model);

 var usuario = _ctx.Usuarios.FirstOrDefault(u => u.Id == id);
 if (usuario == null) return NotFound();

 usuario.Nombre = model.Nombre;
 usuario.Apellidos = model.Apellidos;
 usuario.Email = model.Email;
 usuario.Identificacion = model.Identificacion;

 // Si se proporcionó contraseña, actualizar hash y salt
 if (!string.IsNullOrWhiteSpace(model.Password))
 {
 PasswordHasher.CreatePasswordHash(model.Password, out byte[] hash, out byte[] salt);
 usuario.PasswordHash = hash;
 usuario.PasswordSalt = salt;
 }

 await _ctx.SaveChangesAsync();
 TempData["ToastMessage"] = "Perfil actualizado";
 TempData["ToastType"] = "success";
 return RedirectToAction("Editar", new { id = usuario.Id });
 }
 }
}