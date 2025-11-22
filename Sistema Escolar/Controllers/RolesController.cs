using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Usuarios;
using System.Linq;
using System.Threading.Tasks;
using SistemaEscolar.Interfaces.Usuarios;
using System.Security.Claims;

namespace SistemaEscolar.Controllers
{
 [Authorize(Roles="Administrador")] // solo admin
 [Route("Seguridad/[controller]")]
 public class RolesController : Controller
 {
 private readonly IRolService _roles;
 private readonly ApplicationDbContext _context; // para listas permisos
 public RolesController(IRolService roles, ApplicationDbContext context){ _roles = roles; _context = context; }
 
 private int CurrentUserId()=> int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
 private string Ip()=> HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
 
 [HttpGet]
 [Route("")]
 public async Task<IActionResult> Index(){
 var roles = await _roles.GetAllAsync();
 return View(roles);
 }
 
 [HttpGet("crear")]
 public IActionResult Crear(){
 ViewBag.Permisos = _context.Permisos.OrderBy(p=>p.Codigo).ToList();
 return View();
 }
 
 [HttpPost("crear")]
 public async Task<IActionResult> Crear(RolCreateDTO dto){
 if(!ModelState.IsValid){ ViewBag.Permisos = _context.Permisos.OrderBy(p=>p.Codigo).ToList(); return View(dto); }
 var ok = await _roles.CreateAsync(dto, CurrentUserId(), Ip());
 if(!ok){ ModelState.AddModelError("","No se pudo crear"); ViewBag.Permisos = _context.Permisos.OrderBy(p=>p.Codigo).ToList(); return View(dto);} return RedirectToAction("Index"); }
 
 [HttpGet("editar/{id}")]
 public async Task<IActionResult> Editar(int id){
 var rol = await _roles.GetByIdAsync(id); if(rol==null) return NotFound();
 var dto = new RolUpdateDTO{ Id = rol.Id, Nombre = rol.Nombre, PermisosIds = _context.RolPermisos.Where(rp=>rp.RolId==id).Select(rp=>rp.PermisoId).ToList() };
 ViewBag.Permisos = _context.Permisos.OrderBy(p=>p.Codigo).ToList();
 return View(dto);
 }
 
 [HttpPost("editar/{id}")]
 public async Task<IActionResult> Editar(int id, RolUpdateDTO dto){ if(id!=dto.Id) return BadRequest();
 if(!ModelState.IsValid){ ViewBag.Permisos = _context.Permisos.OrderBy(p=>p.Codigo).ToList(); return View(dto);} var ok = await _roles.UpdateAsync(dto, CurrentUserId(), Ip()); if(!ok){ ModelState.AddModelError("","No se pudo actualizar"); ViewBag.Permisos = _context.Permisos.OrderBy(p=>p.Codigo).ToList(); return View(dto);} return RedirectToAction("Index"); }
 
 [HttpPost("eliminar/{id}")]
 public async Task<IActionResult> Eliminar(int id){ var ok = await _roles.DeleteAsync(id, CurrentUserId(), Ip()); if(!ok) return BadRequest(); return RedirectToAction("Index"); }
 }
}
