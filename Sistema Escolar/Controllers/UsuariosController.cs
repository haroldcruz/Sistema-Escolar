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
 public async Task<IActionResult> Crear(UsuarioCreateDTO dto)
 {
 if (!ModelState.IsValid)
 {
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View(dto);
 }

 var ok = await _usuarios.CreateAsync(dto);
 if (!ok)
 {
 ModelState.AddModelError("", "No se pudo crear");
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View(dto);
 }

 return RedirectToAction("Index");
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
 public async Task<IActionResult> Editar(int id, UsuarioUpdateDTO dto)
 {
 if (id != dto.Id) return BadRequest();

 if (!ModelState.IsValid)
 {
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View(dto);
 }

 var ok = await _usuarios.UpdateAsync(id, dto);
 if (!ok)
 {
 ModelState.AddModelError("", "No se pudo actualizar");
 ViewBag.Roles = _ctx.Roles.OrderBy(r => r.Nombre).ToList();
 return View(dto);
 }

 return RedirectToAction("Index");
 }

 [HttpPost]
 public async Task<IActionResult> Eliminar(int id)
 {
 var ok = await _usuarios.DeleteAsync(id);
 if (!ok) return BadRequest();
 return RedirectToAction("Index");
 }
 }
}