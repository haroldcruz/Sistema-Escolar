using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaEscolar.Controllers
{
 [Authorize(Roles = "Administrador")]
 [Route("Seguridad/[controller]")]
 public class PermisosController : Controller
 {
 private readonly ApplicationDbContext _ctx;
 public PermisosController(ApplicationDbContext ctx) { _ctx = ctx; }

 [HttpGet("")]
 public async Task<IActionResult> Index()
 {
 var list = await _ctx.Permisos.OrderBy(p => p.Codigo).ToListAsync();
 return View(list);
 }

 [HttpGet("crear")]
 public IActionResult Crear()
 {
 return View(new Permiso());
 }

 [HttpPost("crear")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Crear(Permiso model)
 {
 if (!ModelState.IsValid) return View(model);
 // avoid duplicate codigo
 if (await _ctx.Permisos.AnyAsync(p => p.Codigo == model.Codigo))
 {
 ModelState.AddModelError("Codigo", "Código ya existe");
 return View(model);
 }
 _ctx.Permisos.Add(model);
 await _ctx.SaveChangesAsync();
 return RedirectToAction("Index");
 }

 [HttpGet("editar/{id}")]
 public async Task<IActionResult> Editar(int id)
 {
 var p = await _ctx.Permisos.FindAsync(id);
 if (p == null) return NotFound();
 return View(p);
 }

 [HttpPost("editar/{id}")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Editar(int id, Permiso model)
 {
 if (id != model.Id) return BadRequest();
 if (!ModelState.IsValid) return View(model);
 var exists = await _ctx.Permisos.AnyAsync(p => p.Codigo == model.Codigo && p.Id != model.Id);
 if (exists) { ModelState.AddModelError("Codigo", "Código ya existe"); return View(model); }
 var p = await _ctx.Permisos.FindAsync(id);
 if (p == null) return NotFound();
 p.Codigo = model.Codigo;
 p.Descripcion = model.Descripcion;
 await _ctx.SaveChangesAsync();
 return RedirectToAction("Index");
 }

 [HttpPost("eliminar/{id}")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Eliminar(int id)
 {
 var p = await _ctx.Permisos.FindAsync(id);
 if (p == null) return NotFound();
 // ensure no role uses it
 var used = await _ctx.RolPermisos.AnyAsync(rp => rp.PermisoId == id);
 if (used) return BadRequest("El permiso está asignado a roles, desvinculelo antes");
 _ctx.Permisos.Remove(p);
 await _ctx.SaveChangesAsync();
 return RedirectToAction("Index");
 }
 }
}
