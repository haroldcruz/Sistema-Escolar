using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Interfaces.Bitacora;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace SistemaEscolar.Controllers.API
{
 [ApiController]
 [Route("api/bitacora")]
 [Authorize(Policy = "Bitacora.Ver")]
 public class BitacoraApiController : ControllerBase
 {
 private readonly IBitacoraService _bitacora;
 public BitacoraApiController(IBitacoraService bitacora){ _bitacora = bitacora; }

 [HttpGet]
 public async Task<IActionResult> GetAll()
 {
 var items = await _bitacora.GetAllAsync();
 return Ok(items);
 }

 // Devuelve un arreglo directo para compatibilidad con frontend existente
 [HttpGet("paged")]
 public async Task<IActionResult> GetPaged(
 [FromQuery] int page =1,
 [FromQuery] int pageSize =20,
 [FromQuery] string? usuario = null,
 [FromQuery] string? modulo = null,
 [FromQuery] string? accion = null,
 [FromQuery] DateTime? desde = null,
 [FromQuery] DateTime? hasta = null,
 [FromQuery] string? sort = null,
 [FromQuery] string? dir = null)
 {
 var src = await _bitacora.GetAllAsync();
 // Proyección con fecha parseada para poder comparar/ordenar
 var all = src.Select(x => new
 {
 Item = x,
 Usuario = x.Usuario ?? string.Empty,
 Modulo = x.Modulo ?? string.Empty,
 Accion = x.Accion ?? string.Empty,
 Ip = x.Ip ?? string.Empty,
 FechaParsed = TryParseDate(x.Fecha)
 }).AsQueryable();

 if (!string.IsNullOrWhiteSpace(usuario))
 {
 var u = usuario.ToLowerInvariant();
 all = all.Where(x => x.Usuario.ToLowerInvariant().Contains(u));
 }
 if (!string.IsNullOrWhiteSpace(modulo))
 {
 var m = modulo.ToLowerInvariant();
 all = all.Where(x => x.Modulo.ToLowerInvariant().Contains(m));
 }
 if (!string.IsNullOrWhiteSpace(accion))
 {
 var a = accion.ToLowerInvariant();
 all = all.Where(x => x.Accion.ToLowerInvariant().Contains(a));
 }
 if (desde.HasValue) all = all.Where(x => x.FechaParsed >= desde.Value);
 if (hasta.HasValue) all = all.Where(x => x.FechaParsed <= hasta.Value);

 // Orden
 sort = (sort ?? "fecha").ToLowerInvariant();
 dir = (dir ?? "desc").ToLowerInvariant();
 all = sort switch
 {
 "usuario" => (dir=="asc"? all.OrderBy(x=>x.Usuario): all.OrderByDescending(x=>x.Usuario)),
 "accion" => (dir=="asc"? all.OrderBy(x=>x.Accion): all.OrderByDescending(x=>x.Accion)),
 "modulo" => (dir=="asc"? all.OrderBy(x=>x.Modulo): all.OrderByDescending(x=>x.Modulo)),
 "ip" => (dir=="asc"? all.OrderBy(x=>x.Ip): all.OrderByDescending(x=>x.Ip)),
 _ => (dir=="asc"? all.OrderBy(x=>x.FechaParsed): all.OrderByDescending(x=>x.FechaParsed))
 };

 if (page <1) page =1; if (pageSize <1) pageSize =20; if (pageSize >200) pageSize =200;
 var pageData = all.Skip((page -1) * pageSize).Take(pageSize).Select(x => x.Item).ToList();
 return Ok(pageData);
 }

 private static DateTime TryParseDate(string? v)
 {
 if (string.IsNullOrWhiteSpace(v)) return DateTime.MinValue;
 if (DateTime.TryParse(v, out var d)) return d;
 return DateTime.MinValue;
 }
 }
}
