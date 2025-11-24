using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using System.Threading.Tasks;
using System.Linq;

namespace SistemaEscolar.Controllers
{
 [Authorize(Roles = "Administrador")]
 public class MatriculasController : Controller
 {
 private readonly ApplicationDbContext _ctx;
 public MatriculasController(ApplicationDbContext ctx){ _ctx = ctx; }

 [HttpGet]
 public async Task<IActionResult> Crear()
 {
 // Try to resolve the role id for 'Estudiante' to avoid string matching issues
 var estudianteRole = await _ctx.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Nombre == "Estudiante");
 
 object estudiantes;
 if (estudianteRole != null)
 {
 estudiantes = await _ctx.UsuarioRoles
 .Include(ur => ur.Usuario)
 .Where(ur => ur.RolId == estudianteRole.Id)
 .Select(ur => new { ur.Usuario.Id, Nombre = (ur.Usuario.Nombre + " " + ur.Usuario.Apellidos).Trim() })
 .Distinct()
 .OrderBy(u => u.Nombre)
 .ToListAsync();
 }
 else
 {
 // Fallback: try filtering by role name in join (keeps previous behavior if role entry missing)
 estudiantes = await _ctx.UsuarioRoles
 .Include(ur => ur.Rol)
 .Include(ur => ur.Usuario)
 .Where(ur => ur.Rol.Nombre == "Estudiante")
 .Select(ur => new { ur.Usuario.Id, Nombre = (ur.Usuario.Nombre + " " + ur.Usuario.Apellidos).Trim() })
 .Distinct()
 .OrderBy(u => u.Nombre)
 .ToListAsync();
 }

 var cuatrimestres = await _ctx.Cuatrimestres.AsNoTracking().OrderBy(c=>c.Nombre).Select(c=> new { c.Id, c.Nombre }).ToListAsync();
 ViewBag.Estudientes = estudiantes;
 ViewBag.Cuatrimestres = cuatrimestres;
 return View();
 }
 }
}
