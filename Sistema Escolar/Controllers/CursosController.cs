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
 // Administrador y Coordinador
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
 var curso = await _cursos.GetByIdAsync(id);
 if (curso == null) return NotFound();
 var entity = await _ctx.Cursos.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id);
 var dto = new CursoUpdateDTO
 {
 Codigo = entity!.Codigo,
 Nombre = entity!.Nombre,
 Descripcion = entity!.Descripcion,
 Creditos = entity!.Creditos,
 CuatrimestreId = entity!.CuatrimestreId
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
 ModelState.AddModelError("","No se pudo actualizar (código duplicado u otros)");
 ViewBag.Cuatrimestres = _ctx.Cuatrimestres.AsNoTracking().OrderBy(c=>c.Nombre).ToList();
 return View(dto);
 }
 return RedirectToAction("Index");
 }
 }
}