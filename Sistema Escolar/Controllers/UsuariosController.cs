using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using SistemaEscolar.Helpers;
using System.Linq;
using SistemaEscolar.DTOs.Usuarios;
using Microsoft.EntityFrameworkCore;

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

 // GET: /Usuarios
 [HttpGet("")]
 [Authorize(Policy = "Usuarios.Gestion")]
 public async Task<IActionResult> Index()
 {
 var usuarios = await _ctx.Usuarios
 .AsNoTracking()
 .Select(u => new UsuarioDTO
 {
 Id = u.Id,
 NombreCompleto = (u.Nombre + " " + u.Apellidos).Trim(),
 Email = u.Email,
 Identificacion = u.Identificacion,
 Roles = _ctx.UsuarioRoles.Where(ur => ur.UsuarioId == u.Id).Join(_ctx.Roles, ur => ur.RolId, r => r.Id, (ur, r) => r.Nombre).ToList()
 }).ToListAsync();

 ViewBag.UsuariosCount = usuarios.Count;
 return View(usuarios);
 }

 // GET: /Usuarios/Crear
 [HttpGet("Crear")]
 [Authorize(Policy = "Usuarios.Gestion")]
 public IActionResult Crear()
 {
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View();
 }

 // POST: /Usuarios/Crear
 [HttpPost("Crear")]
 [ValidateAntiForgeryToken]
 [Authorize(Policy = "Usuarios.Gestion")]
 public async Task<IActionResult> CrearPost(UsuarioCreateDTO model)
 {
 if (!ModelState.IsValid)
 {
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View("Crear", model);
 }

 // Validate uniqueness
 if (await _ctx.Usuarios.AnyAsync(u => u.Email == model.Email))
 {
 ModelState.AddModelError(nameof(model.Email), "El email ya está en uso");
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View("Crear", model);
 }
 if (await _ctx.Usuarios.AnyAsync(u => u.Identificacion == model.Identificacion))
 {
 ModelState.AddModelError(nameof(model.Identificacion), "La identificación ya está en uso");
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View("Crear", model);
 }

 var usuario = new Models.Usuario
 {
 Nombre = model.Nombre,
 Apellidos = model.Apellidos,
 Email = model.Email,
 Identificacion = model.Identificacion,
 IsActive = true
 };

 // Hash password
 PasswordHasher.CreatePasswordHash(model.Password, out byte[] hash, out byte[] salt);
 usuario.PasswordHash = hash;
 usuario.PasswordSalt = salt;

 _ctx.Usuarios.Add(usuario);
 await _ctx.SaveChangesAsync();

 // Assign roles if provided
 if (model.RolesIds != null && model.RolesIds.Any())
 {
 foreach (var rid in model.RolesIds.Distinct())
 {
 // ensure role exists
 if (await _ctx.Roles.AnyAsync(r => r.Id == rid))
 _ctx.UsuarioRoles.Add(new Models.UsuarioRol { UsuarioId = usuario.Id, RolId = rid });
 }
 await _ctx.SaveChangesAsync();
 }

 TempData["ToastMessage"] = "Usuario creado";
 TempData["ToastType"] = "success";
 return RedirectToAction("Index");
 }

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
 // supply roles for view
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 var selected = _ctx.UsuarioRoles.Where(ur => ur.UsuarioId == id).Select(ur => ur.RolId).ToList();
 model.RolesIds = selected;
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
 if (!ModelState.IsValid)
 {
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View("Editar", model);
 }

 var usuario = _ctx.Usuarios.FirstOrDefault(u => u.Id == id);
 if (usuario == null) return NotFound();

 usuario.Nombre = model.Nombre;
 usuario.Apellidos = model.Apellidos;
 usuario.Email = model.Email;
 usuario.Identificacion = model.Identificacion;

 // Si se proporcionó contraseña, actualizar hash y salt
 var newPassword = model.NewPassword ?? model.Password;
 if (!string.IsNullOrWhiteSpace(newPassword))
 {
 PasswordHasher.CreatePasswordHash(newPassword, out byte[] hash, out byte[] salt);
 usuario.PasswordHash = hash;
 usuario.PasswordSalt = salt;
 }

 // update roles if admin
 if (IsAdmin())
 {
 var currentRoles = _ctx.UsuarioRoles.Where(ur => ur.UsuarioId == id).ToList();
 _ctx.UsuarioRoles.RemoveRange(currentRoles);
 if (model.RolesIds != null && model.RolesIds.Any())
 {
 foreach (var rid in model.RolesIds.Distinct())
 {
 _ctx.UsuarioRoles.Add(new Models.UsuarioRol { UsuarioId = id, RolId = rid });
 }
 }
 }

 await _ctx.SaveChangesAsync();
 TempData["ToastMessage"] = "Perfil actualizado";
 TempData["ToastType"] = "success";
 return RedirectToAction("Editar", new { id = usuario.Id });
 }
 }
}