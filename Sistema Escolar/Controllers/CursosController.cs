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
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Editar(int id, CursoUpdateDTO dto)
 {
 if (id != dto.Id) return BadRequest();

 if (!ModelState.IsValid)
 {
 ViewBag.Cuatrimestres = _ctx.Cuatrimestres.OrderBy(c=>c.Nombre).ToList();
 return View(dto);
 }

 try
 {
 var (ok, error) = await _cursos.UpdateAsync(id, dto, CurrentUserId(), Ip());
 if (!ok)
 {
 TempData["ToastMessage"] = error ?? "No se pudo actualizar el curso. Revise los datos e intente de nuevo.";
 TempData["ToastType"] = "danger";
 ViewBag.Cuatrimestres = _ctx.Cuatrimestres.OrderBy(c=>c.Nombre).ToList();
 return View(dto);
 }

 TempData["ToastMessage"] = "Curso actualizado correctamente.";
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
 Accion = "Error actualizar curso: " + ex.Message,
 Modulo = "Academico",
 Ip = Ip()
 });
 await _ctx.SaveChangesAsync();
 }
 catch { }

 TempData["ToastMessage"] = "Error interno del servidor. Intente de nuevo más tarde.";
 TempData["ToastType"] = "danger";
 ViewBag.Cuatrimestres = _ctx.Cuatrimestres.OrderBy(c=>c.Nombre).ToList();
 return View(dto);
 }
 }
 }
}