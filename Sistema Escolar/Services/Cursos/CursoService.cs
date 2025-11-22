using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Cursos;
using SistemaEscolar.Interfaces.Cursos;
using SistemaEscolar.Models.Academico;
using System.Linq;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Http;
using SistemaEscolar.Interfaces.Bitacora;

namespace SistemaEscolar.Services.Cursos
{
 // Implementación del servicio de cursos
 public class CursoService : ICursoService
 {
 private readonly ApplicationDbContext _context;
 private readonly IHttpContextAccessor _http;
 private readonly IBitacoraService _bitacora;

 public CursoService(ApplicationDbContext context, IHttpContextAccessor http, IBitacoraService bitacora)
 {
 _context = context;
 _http = http;
 _bitacora = bitacora;
 }

 private int GetUserId() => int.Parse(_http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
 private string GetIp() => _http.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";

 // Lista de cursos
 public async Task<IEnumerable<CursoDTO>> GetAllAsync()
 {
 return await _context.Cursos
 .Include(c => c.Cuatrimestre)
 .Include(c => c.CursoDocentes).ThenInclude(cd => cd.Docente)
 .Select(c => new CursoDTO
 {
 Id = c.Id,
 Codigo = c.Codigo,
 Nombre = c.Nombre,
 Descripcion = c.Descripcion,
 Creditos = c.Creditos,
 Cuatrimestre = c.Cuatrimestre != null ? c.Cuatrimestre.Nombre : null,
 CuatrimestreId = c.CuatrimestreId,
 Docentes = c.CursoDocentes.Select(d => d.Docente.Nombre + " " + d.Docente.Apellidos).ToList()
 })
 .ToListAsync();
 }

 // Detalle de curso
 public async Task<CursoDTO> GetByIdAsync(int id)
 {
 var c = await _context.Cursos
 .Include(cu => cu.Cuatrimestre)
 .Include(d => d.CursoDocentes).ThenInclude(x => x.Docente)
 .FirstOrDefaultAsync(cu => cu.Id == id);
 if (c == null)
 return null!;
 return new CursoDTO
 {
 Id = c.Id,
 Codigo = c.Codigo,
 Nombre = c.Nombre,
 Descripcion = c.Descripcion,
 Creditos = c.Creditos,
 CuatrimestreId = c.CuatrimestreId,
 Cuatrimestre = c.Cuatrimestre != null ? c.Cuatrimestre.Nombre : null,
 Docentes = c.CursoDocentes.Select(x => x.Docente.Nombre + " " + x.Docente.Apellidos).ToList()
 };
 }

 // Crear curso
 public async Task<bool> CreateAsync(CursoCreateDTO dto)
 {
 var exists = await _context.Cursos.AnyAsync(x => x.Codigo == dto.Codigo);
 if (exists) return false;
 var userId = GetUserId();
 var entity = new Curso
 {
 Codigo = dto.Codigo,
 Nombre = dto.Nombre,
 Descripcion = dto.Descripcion,
 Creditos = dto.Creditos,
 CuatrimestreId = dto.CuatrimestreId,
 FechaCreacion = DateTime.UtcNow,
 UsuarioCreacion = userId
 };
 _context.Cursos.Add(entity);
 await _context.SaveChangesAsync();
 await _bitacora.RegistrarAsync(userId, $"Curso creado {entity.Codigo}", "Cursos", GetIp());
 return true;
 }

 // Actualizar curso
 public async Task<bool> UpdateAsync(int id, CursoUpdateDTO dto)
 {
 var c = await _context.Cursos.FindAsync(id);
 if (c == null) return false;
 bool tieneMatriculas = await _context.Matriculas.AnyAsync(m => m.CursoId == id);
 if (!string.Equals(c.Codigo, dto.Codigo, StringComparison.OrdinalIgnoreCase))
 {
 if (tieneMatriculas) return false;
 var exists = await _context.Cursos.AnyAsync(x => x.Codigo == dto.Codigo && x.Id != id);
 if (exists) return false;
 c.Codigo = dto.Codigo;
 }
 c.Nombre = dto.Nombre;
 c.Descripcion = dto.Descripcion;
 c.Creditos = dto.Creditos;
 c.CuatrimestreId = dto.CuatrimestreId;
 c.UsuarioModificacion = GetUserId();
 c.FechaModificacion = DateTime.UtcNow;
 await _context.SaveChangesAsync();
 await _bitacora.RegistrarAsync(c.UsuarioModificacion ??0, $"Curso modificado {c.Codigo}", "Cursos", GetIp());
 return true;
 }

 public async Task<bool> PuedeEliminarAsync(int cursoId)
 {
 return !await _context.Matriculas.AnyAsync(m => m.CursoId == cursoId);
 }

 // Eliminar curso (solo si no tiene matrículas)
 public async Task<bool> DeleteAsync(int id)
 {
 var c = await _context.Cursos.FindAsync(id);
 if (c == null) return false;
 if (!await PuedeEliminarAsync(id)) return false;
 _context.Cursos.Remove(c);
 await _context.SaveChangesAsync();
 await _bitacora.RegistrarAsync(GetUserId(), $"Curso eliminado {c.Codigo}", "Cursos", GetIp());
 return true;
 }

 // Asignar docente
 public async Task<bool> AsignarDocenteAsync(CursoDocenteDTO dto)
 {
 var curso = await _context.Cursos.FindAsync(dto.CursoId);
 if (curso == null) return false;
 var existe = await _context.CursoDocentes.AnyAsync(x => x.CursoId == dto.CursoId && x.DocenteId == dto.DocenteId);
 if (existe) return true; // ya asignado
 _context.CursoDocentes.Add(new CursoDocente { CursoId = dto.CursoId, DocenteId = dto.DocenteId, Activo = true });
 await _context.SaveChangesAsync();
 await _bitacora.RegistrarAsync(GetUserId(), $"Docente {dto.DocenteId} asignado curso {dto.CursoId}", "Cursos", GetIp());
 return true;
 }

 public async Task<bool> RemoverDocenteAsync(int cursoId, int docenteId)
 {
 var rel = await _context.CursoDocentes.FirstOrDefaultAsync(x => x.CursoId == cursoId && x.DocenteId == docenteId);
 if (rel == null) return false;
 _context.CursoDocentes.Remove(rel);
 await _context.SaveChangesAsync();
 await _bitacora.RegistrarAsync(GetUserId(), $"Docente {docenteId} removido curso {cursoId}", "Cursos", GetIp());
 return true;
 }
 }
}
