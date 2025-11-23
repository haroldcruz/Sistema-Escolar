using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Cursos;
using SistemaEscolar.Interfaces.Cursos;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaEscolar.Controllers
{
 // Administrador y Coordinador: CRUD cursos
 [Authorize(Roles = "Administrador,Coordinador")]
 public class CursosController : Controller
 {
 private readonly ICursoService _cursos;
 private readonly ApplicationDbContext _ctx;
 public CursosController(ICursoService cursos, ApplicationDbContext ctx){ _cursos = cursos; _ctx = ctx; }
 private int CurrentUserId()=> int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
 private string Ip()=> HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

 public IActionResult Index() => View();

 [Authorize(Policy = "Cursos.Crear")]
 public IActionResult Crear(){
 ViewBag.Cuatrimestres = _ctx.Cuatrimestres.AsNoTracking().OrderBy(c=>c.Nombre).ToList();
 return View(new CursoCreateDTO());
 }

 [HttpPost]
 [Authorize(Policy = "Cursos.Crear")]
 public async Task<IActionResult> Crear(CursoCreateDTO dto){
 if(!ModelState.IsValid){ ViewBag.Cuatrimestres = _ctx.Cuatrimestres.AsNoTracking().OrderBy(c=>c.Nombre).ToList(); return View(dto);} 
 var ok = await _cursos.CreateAsync(dto, CurrentUserId(), Ip());
 if(!ok){ ModelState.AddModelError("","Código duplicado u otros errores"); ViewBag.Cuatrimestres = _ctx.Cuatrimestres.AsNoTracking().OrderBy(c=>c.Nombre).ToList(); return View(dto);} 
 return RedirectToAction("Index");
 }

 [Authorize(Policy = "Cursos.Editar")]
 public async Task<IActionResult> Editar(int id)
 {
 var entity = await _ctx.Cursos.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id);
 if (entity==null) return NotFound();
 var dto = new CursoUpdateDTO
 {
 Codigo = entity.Codigo,
 Nombre = entity.Nombre,
 Descripcion = entity.Descripcion,
 Creditos = entity.Creditos,
 CuatrimestreId = entity.CuatrimestreId
 };
 ViewBag.Cuatrimestres = _ctx.Cuatrimestres.AsNoTracking().OrderBy(c=>c.Nombre).ToList();
 return View(dto);
 }

 [HttpPost]
 [Authorize(Policy = "Cursos.Editar")]
 public async Task<IActionResult> Editar(int id, CursoUpdateDTO dto)
 {
 if (!ModelState.IsValid)
 {
 ViewBag.Cuatrimestres = _ctx.Cuatrimestres.AsNoTracking().OrderBy(c=>c.Nombre).ToList();
 return View(dto);
 }
 var ok = await _cursos.UpdateAsync(id, dto, CurrentUserId(), Ip());
 if(!ok)
 {
 var tieneMatriculas = await _ctx.Matriculas.AnyAsync(m => m.CursoId == id);
 var msg = tieneMatriculas ? "No se puede cambiar el código porque hay estudiantes inscritos." : "No se pudo actualizar (código duplicado u otros).";
 ModelState.AddModelError("", msg);
 ViewBag.Cuatrimestres = _ctx.Cuatrimestres.AsNoTracking().OrderBy(c=>c.Nombre).ToList();
 return View(dto);
 }
 return RedirectToAction("Index");
 }

 // Vista de asignar docentes por curso se mantiene en ruta separada
 [Authorize(Policy = "Cursos.AsignarDocente")]
 public async Task<IActionResult> AsignarDocentes(int id)
 {
 var curso = await _ctx.Cursos.AsNoTracking().FirstOrDefaultAsync(c=>c.Id==id);
 if (curso==null) return NotFound();
 ViewBag.CursoId = id;
 ViewBag.CursoNombre = $"{curso.Codigo} - {curso.Nombre}";
 var docentes = await (
 from u in _ctx.Usuarios
 join ur in _ctx.UsuarioRoles on u.Id equals ur.UsuarioId
 join r in _ctx.Roles on ur.RolId equals r.Id
 where r.Nombre == "Docente"
 orderby u.Nombre
 select new { u.Id, NombreCompleto = u.Nombre + " " + u.Apellidos }
 ).Distinct().ToListAsync();
 ViewBag.Docentes = docentes;
 return View();
 }
 }
}