using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Interfaces.Usuarios;
using SistemaEscolar.DTOs.Usuarios;
using System.Threading.Tasks;
using System.Security.Claims;
using SistemaEscolar.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SistemaEscolar.Controllers
{
 // Vista de usuarios protegida por permiso granular
 [Authorize(Policy = "Usuarios.Gestion")]
 public class UsuariosController : Controller
 {
 private readonly IUsuarioService _usuarios;
 private readonly ApplicationDbContext _ctx;

 public UsuariosController(IUsuarioService usuarios, ApplicationDbContext ctx)
 {
 _usuarios = usuarios;
 _ctx = ctx;
 }

 private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
 private string Ip() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

 public async Task<IActionResult> Index()
 {
 var lista = await _usuarios.GetAllAsync();
 return View(lista);
 }

 public IActionResult Crear()
 {
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View(new UsuarioCreateDTO());
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Crear(UsuarioCreateDTO dto)
 {
 if (!ModelState.IsValid)
 {
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View(dto);
 }

 try
 {
 var (ok, error) = await _usuarios.CreateAsync(dto);
 if (!ok)
 {
 TempData["ToastMessage"] = error ?? "No se pudo crear el usuario. Revise los datos e intente de nuevo.";
 TempData["ToastType"] = "danger";
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View(dto);
 }

 TempData["ToastMessage"] = "Usuario creado correctamente.";
 TempData["ToastType"] = "success";
 return RedirectToAction("Index");
 }
 catch (System.Exception ex)
 {
 try
 {
 await _ctx.BitacoraEntries.AddAsync(new Models.Bitacora.BitacoraEntry
 {
 UsuarioId = CurrentUserId(),
 Accion = "Error crear usuario: " + ex.Message,
 Modulo = "Seguridad",
 Ip = Ip()
 });
 await _ctx.SaveChangesAsync();
 }
 catch { }

 TempData["ToastMessage"] = "Error interno del servidor. Intente de nuevo más tarde.";
 TempData["ToastType"] = "danger";
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View(dto);
 }
 }

 public async Task<IActionResult> Editar(int id)
 {
 var u = await _usuarios.GetByIdAsync(id);
 if (u == null) return NotFound();

 var nombres = u.NombreCompleto.Split(' ');
 var nombre = nombres.First();
 var apellidos = string.Join(' ', nombres.Skip(1));

 var dto = new UsuarioUpdateDTO
 {
 Id = u.Id,
 Nombre = nombre,
 Apellidos = apellidos,
 Email = u.Email,
 Identificacion = u.Identificacion,
 RolesIds = await _ctx.UsuarioRoles.Where(ur => ur.UsuarioId == id).Select(ur => ur.RolId).ToListAsync()
 };

 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View(dto);
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Editar(int id, UsuarioUpdateDTO dto)
 {
 if (id != dto.Id) return BadRequest();

 if (!ModelState.IsValid)
 {
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View(dto);
 }

 try
 {
 var (ok, error) = await _usuarios.UpdateAsync(id, dto);
 if (!ok)
 {
 TempData["ToastMessage"] = error ?? "No se pudo actualizar el usuario. Revise los datos e intente de nuevo.";
 TempData["ToastType"] = "danger";
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View(dto);
 }

 TempData["ToastMessage"] = "Usuario actualizado correctamente.";
 TempData["ToastType"] = "success";
 return RedirectToAction("Index");
 }
 catch (System.Exception ex)
 {
 try
 {
 await _ctx.BitacoraEntries.AddAsync(new Models.Bitacora.BitacoraEntry
 {
 UsuarioId = CurrentUserId(),
 Accion = "Error actualizar usuario: " + ex.Message,
 Modulo = "Seguridad",
 Ip = Ip()
 });
 await _ctx.SaveChangesAsync();
 }
 catch { }

 TempData["ToastMessage"] = "Error interno del servidor. Intente de nuevo más tarde.";
 TempData["ToastType"] = "danger";
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View(dto);
 }
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Eliminar(int id)
 {
 try
 {
 var (ok, error) = await _usuarios.DeleteAsync(id);
 if (!ok)
 {
 TempData["ToastMessage"] = error ?? "No se pudo eliminar el usuario.";
 TempData["ToastType"] = "warning";
 return RedirectToAction("Index");
 }

 TempData["ToastMessage"] = "Usuario eliminado (desactivado) correctamente.";
 TempData["ToastType"] = "success";
 return RedirectToAction("Index");
 }
 catch (System.Exception ex)
 {
 try
 {
 await _ctx.BitacoraEntries.AddAsync(new Models.Bitacora.BitacoraEntry
 {
 UsuarioId = CurrentUserId(),
 Accion = "Error eliminar usuario: " + ex.Message,
 Modulo = "Seguridad",
 Ip = Ip()
 });
 await _ctx.SaveChangesAsync();
 }
 catch { }

 TempData["ToastMessage"] = "Error interno del servidor. Intente de nuevo más tarde.";
 TempData["ToastType"] = "danger";
 return RedirectToAction("Index");
 }
 }
 }
}