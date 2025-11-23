using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.Interfaces.Cursos;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaEscolar.Controllers
{
 [Authorize(Policy = "Cursos.AsignarDocente")]
 [Route("Docentes")] // módulo docente-céntrico
 public class DocentesController : Controller
 {
 private readonly ApplicationDbContext _ctx;
 private readonly ICursoService _cursos;
 public DocentesController(ApplicationDbContext ctx, ICursoService cursos){ _ctx = ctx; _cursos = cursos; }
 private string Ip()=> HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

 [HttpGet("")]
 public async Task<IActionResult> Index(int? id)
 {
 // lista de docentes (usuarios rol Docente)
 var docentes = await (
 from u in _ctx.Usuarios
 join ur in _ctx.UsuarioRoles on u.Id equals ur.UsuarioId
 join r in _ctx.Roles on ur.RolId equals r.Id
 where r.Nombre == "Docente"
 orderby u.Nombre
 select new { u.Id, NombreCompleto = u.Nombre + " " + u.Apellidos }
 ).Distinct().ToListAsync();
 ViewBag.Docentes = docentes;
 ViewBag.DocenteId = id;
 return View();
 }
 }
}
