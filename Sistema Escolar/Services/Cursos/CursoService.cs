using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Cursos;
using SistemaEscolar.Interfaces.Bitacora;
using SistemaEscolar.Interfaces.Cursos;
using SistemaEscolar.Models.Academico;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SistemaEscolar.Services.Cursos
{
 public class CursoService : ICursoService
 {
 private readonly ApplicationDbContext _ctx;
 private readonly IBitacoraService _bitacora;
 public CursoService(ApplicationDbContext ctx, IBitacoraService bitacora){ _ctx = ctx; _bitacora = bitacora; }

 public async Task<IEnumerable<CursoDTO>> GetAllAsync()
 {
 var cursos = await _ctx.Cursos
 .Include(c=>c.Cuatrimestre)
 .Include(c=>c.CursoDocentes).ThenInclude(cd=>cd.Docente)
 .OrderBy(c=>c.Nombre)
 .ToListAsync();
 return cursos.Select(c => new CursoDTO{
 Id = c.Id,
 Codigo = c.Codigo,
 Nombre = c.Nombre,
 Descripcion = c.Descripcion,
 Creditos = c.Creditos,
 Cuatrimestre = c.Cuatrimestre != null ? c.Cuatrimestre.Nombre : null,
 Docentes = c.CursoDocentes.Select(cd => cd.Docente.Nombre + " " + cd.Docente.Apellidos).ToList()
 });
 }

 public async Task<CursoDTO?> GetByIdAsync(int id)
 {
 var c = await _ctx.Cursos
 .Include(x=>x.Cuatrimestre)
 .Include(x=>x.CursoDocentes).ThenInclude(cd=>cd.Docente)
 .FirstOrDefaultAsync(x=>x.Id==id);
 if (c==null) return null;
 return new CursoDTO{
 Id = c.Id,
 Codigo = c.Codigo,
 Nombre = c.Nombre,
 Descripcion = c.Descripcion,
 Creditos = c.Creditos,
 Cuatrimestre = c.Cuatrimestre?.Nombre,
 Docentes = c.CursoDocentes.Select(cd => cd.Docente.Nombre + " " + cd.Docente.Apellidos).ToList()
 };
 }

 public async Task<bool> CreateAsync(CursoCreateDTO dto, int usuarioId, string ip)
 {
 dto.Codigo = (dto.Codigo ?? string.Empty).Trim();
 dto.Nombre = (dto.Nombre ?? string.Empty).Trim();
 if (string.IsNullOrWhiteSpace(dto.Codigo) || string.IsNullOrWhiteSpace(dto.Nombre)) return false;
 if (await _ctx.Cursos.AnyAsync(c => c.Codigo == dto.Codigo)) return false; // duplicado

 var c = new Curso{
 Codigo = dto.Codigo,
 Nombre = dto.Nombre,
 Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
 Creditos = dto.Creditos,
 CuatrimestreId = dto.CuatrimestreId,
 FechaCreacion = DateTime.UtcNow,
 CreadoPorId = usuarioId
 };
 _ctx.Cursos.Add(c);
 await _ctx.SaveChangesAsync();
 await _bitacora.RegistrarAsync(usuarioId, $"Crear curso {c.Codigo} - {c.Nombre}", "Cursos", ip);
 return true;
 }

 public async Task<bool> UpdateAsync(int id, CursoUpdateDTO dto, int usuarioId, string ip)
 {
 var c = await _ctx.Cursos.FirstOrDefaultAsync(x=>x.Id==id);
 if (c==null) return false;
 // Validar cambio de código
 var nuevoCodigo = (dto.Codigo ?? string.Empty).Trim();
 if (string.IsNullOrWhiteSpace(nuevoCodigo)) return false;
 if (!string.Equals(c.Codigo, nuevoCodigo, StringComparison.OrdinalIgnoreCase))
 {
 if (await _ctx.Cursos.AnyAsync(x=>x.Codigo==nuevoCodigo && x.Id!=id)) return false;
 c.Codigo = nuevoCodigo;
 }
 c.Nombre = (dto.Nombre ?? string.Empty).Trim();
 c.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
 c.Creditos = dto.Creditos;
 c.CuatrimestreId = dto.CuatrimestreId;
 await _ctx.SaveChangesAsync();
 await _bitacora.RegistrarAsync(usuarioId, $"Actualizar curso {c.Codigo}", "Cursos", ip);
 return true;
 }

 public async Task<bool> DeleteAsync(int id, int usuarioId, string ip)
 {
 var c = await _ctx.Cursos.Include(x=>x.Matriculas).Include(x=>x.CursoDocentes).FirstOrDefaultAsync(x=>x.Id==id);
 if (c==null) return false;
 if (c.Matriculas.Any()) return false; // no eliminar si tiene matrículas
 _ctx.Cursos.Remove(c);
 await _ctx.SaveChangesAsync();
 await _bitacora.RegistrarAsync(usuarioId, $"Eliminar curso {c.Codigo}", "Cursos", ip);
 return true;
 }
 }
}
