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
 public class RolPermisosController : Controller
 {
 private readonly ApplicationDbContext _ctx;
 public RolPermisosController(ApplicationDbContext ctx){ _ctx = ctx; }

 [HttpGet("{rolId}")]
 public async Task<IActionResult> Edit(int rolId)
 {
 var rol = await _ctx.Roles.FindAsync(rolId);
 if(rol==null) return NotFound();
 var permisos = await _ctx.Permisos.OrderBy(p=>p.Codigo).ToListAsync();
 var asignados = await _ctx.RolPermisos.Where(rp=>rp.RolId==rolId).Select(rp=>rp.PermisoId).ToListAsync();
 ViewBag.Rol = rol;
 ViewBag.Permisos = permisos;
 ViewBag.Asignados = asignados;
 return View();
 }

 [HttpPost("{rolId}")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Edit(int rolId, int[] permisoIds)
 {
 var rol = await _ctx.Roles.FindAsync(rolId); if(rol==null) return NotFound();
 // remove exisiting
 var actuales = _ctx.RolPermisos.Where(rp=>rp.RolId==rolId);
 _ctx.RolPermisos.RemoveRange(actuales);
 // add selected
 foreach(var pid in permisoIds ?? new int[0]) _ctx.RolPermisos.Add(new RolPermiso{ RolId = rolId, PermisoId = pid });
 await _ctx.SaveChangesAsync();
 return RedirectToAction("Index","Roles");
 }
 }
}
