using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Cursos;
using SistemaEscolar.Interfaces.Cursos;
using SistemaEscolar.Interfaces.Bitacora;
using SistemaEscolar.Models.Academico;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace SistemaEscolar.Services.Cursos
{
 public class CursoService : ICursoService
 {
 private readonly ApplicationDbContext _ctx;
 private readonly IBitacoraService _bitacora;

 public CursoService(ApplicationDbContext ctx, IBitacoraService bitacora)
 {
 _ctx = ctx;
 _bitacora = bitacora;
 }

 public async Task<IEnumerable<CursoDTO>> GetAllAsync()
 {
 var cursos = await _ctx.Cursos
 .Include(c => c.Cuatrimestre)
 .Include(c => c.CursoDocentes).ThenInclude(cd => cd.Docente)
 .OrderBy(c => c.Nombre)
 .ToListAsync();

 return cursos.Select(c => new CursoDTO
 {
 Id = c.Id,
 Codigo = c.Codigo,
 Nombre = c.Nombre,
 Descripcion = c.Descripcion,
 Creditos = c.Creditos,
 Cuatrimestre = c.Cuatrimestre != null ? c.Cuatrimestre.Nombre : null,
 CuatrimestreNumero = c.Cuatrimestre != null ? c.Cuatrimestre.Numero : null,
 Docentes = c.CursoDocentes.Select(cd => cd.Docente.Nombre + " " + cd.Docente.Apellidos).ToList()
 });
 }

 public async Task<CursoDTO?> GetByIdAsync(int id)
 {
 var c = await _ctx.Cursos
 .Include(x => x.Cuatrimestre)
 .Include(x => x.CursoDocentes).ThenInclude(cd => cd.Docente)
 .FirstOrDefaultAsync(x => x.Id == id);

 if (c == null) return null;

 return new CursoDTO
 {
 Id = c.Id,
 Codigo = c.Codigo,
 Nombre = c.Nombre,
 Descripcion = c.Descripcion,
 Creditos = c.Creditos,
 Cuatrimestre = c.Cuatrimestre?.Nombre,
 CuatrimestreNumero = c.Cuatrimestre?.Numero,
 Docentes = c.CursoDocentes.Select(cd => cd.Docente.Nombre + " " + cd.Docente.Apellidos).ToList()
 };
 }

 public async Task<bool> CreateAsync(CursoCreateDTO dto, int usuarioId, string ip)
 {
 dto.Codigo = (dto.Codigo ?? string.Empty).Trim();
 dto.Nombre = (dto.Nombre ?? string.Empty).Trim();
 if (string.IsNullOrWhiteSpace(dto.Codigo) || string.IsNullOrWhiteSpace(dto.Nombre)) return false;
 if (await _ctx.Cursos.AnyAsync(c => c.Codigo == dto.Codigo)) return false; // duplicado

 var c = new Curso
 {
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
 try { await _bitacora.RegistrarAsync(usuarioId, $"Crear curso {c.Codigo} - {c.Nombre}", "Cursos", ip); } catch { }
 return true;
 }

 public async Task<(bool ok, string? error)> UpdateAsync(int id, CursoUpdateDTO dto, int usuarioId, string ip)
 {
 var c = await _ctx.Cursos.Include(x => x.Matriculas).FirstOrDefaultAsync(x => x.Id == id);
 if (c == null) return (false, "Curso no encontrado");

 // Si el código cambia, verificar que no existan estudiantes matriculados
 var codigoCambiado = !string.Equals(c.Codigo, dto.Codigo ?? string.Empty, StringComparison.OrdinalIgnoreCase);
 if (codigoCambiado)
 {
 var tieneMatriculas = await _ctx.Matriculas.AnyAsync(m => m.CursoId == id && m.Activo);
 if (tieneMatriculas)
 {
 return (false, "No se puede cambiar el código del curso porque tiene estudiantes matriculados");
 }
 }

 // Validar duplicados de código (excluyendo el propio curso)
 if (await _ctx.Cursos.AnyAsync(x => x.Codigo == dto.Codigo && x.Id != id)) return (false, "Código duplicado");

 // Actualizar campos editables
 c.Nombre = dto.Nombre;
 c.Descripcion = dto.Descripcion ?? string.Empty;
 c.Creditos = dto.Creditos;
 c.CuatrimestreId = dto.CuatrimestreId;
 if (codigoCambiado) c.Codigo = dto.Codigo;

 c.FechaModificacion = DateTime.UtcNow;
 c.ModificadoPorId = usuarioId;

 try
 {
 await _ctx.SaveChangesAsync();
 try { await _bitacora.RegistrarAsync(usuarioId, $"Actualizar curso {c.Codigo}", "Academico", ip); } catch { }
 return (true, null);
 }
 catch (DbUpdateException)
 {
 return (false, "Error al guardar en la base de datos");
 }
 }

 public async Task<bool> DeleteAsync(int id, int usuarioId, string ip)
 {
 var c = await _ctx.Cursos.FindAsync(id);
 if (c == null) return false;
 // Eliminar solo si no hay matriculas activas: esta verificación en controlador, pero repetir para seguridad
 var tieneMatriculas = await _ctx.Matriculas.AnyAsync(m => m.CursoId == id && m.Activo);
 if (tieneMatriculas) return false;
 _ctx.Cursos.Remove(c);
 await _ctx.SaveChangesAsync();
 try { await _bitacora.RegistrarAsync(usuarioId, $"Eliminar curso {c.Codigo}", "Academico", ip); } catch { }
 return true;
 }

 // Asignación de docentes con validación de conflicto de horarios
 public async Task<(bool ok, string? error)> AsignarDocenteAsync(int cursoId, int docenteId, int usuarioId, string ip)
 {
 if (!await _ctx.Cursos.AnyAsync(c => c.Id == cursoId)) return (false, "Curso no encontrado");
 if (!await _ctx.Usuarios.AnyAsync(u => u.Id == docenteId)) return (false, "Docente no encontrado");
 if (await _ctx.CursoDocentes.AnyAsync(cd => cd.CursoId == cursoId && cd.DocenteId == docenteId)) return (false, "Ya asignado");

 // Obtener cuatrimestre y horarios del curso objetivo
 var targetCourse = await _ctx.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cursoId);
 if (targetCourse == null) return (false, "Curso no encontrado");
 var targetCuatrimestreId = targetCourse.CuatrimestreId;
 var horariosCurso = await _ctx.HorariosCurso.Where(h => h.CursoId == cursoId).ToListAsync();

 if (horariosCurso.Any())
 {
 // Obtener ids de cursos donde el docente ya está asignado
 var assignedCourseIds = await _ctx.CursoDocentes.Where(cd => cd.DocenteId == docenteId).Select(cd => cd.CursoId).ToListAsync();

 if (assignedCourseIds.Any())
 {
 // Si el curso objetivo tiene cuatrimestre, limitar la comprobación a cursos del mismo cuatrimestre
 if (targetCuatrimestreId.HasValue)
 {
 assignedCourseIds = await _ctx.Cursos
 .Where(c => assignedCourseIds.Contains(c.Id) && c.CuatrimestreId == targetCuatrimestreId.Value)
 .Select(c => c.Id)
 .ToListAsync();
 }

 if (assignedCourseIds.Any())
 {
 // Obtener horarios de los cursos asignados filtrados
 var otherHorarios = await _ctx.HorariosCurso
 .Where(h => assignedCourseIds.Contains(h.CursoId))
 .Include(h => h.Curso)
 .ToListAsync();

 foreach (var h in horariosCurso)
 {
 var conflictEntry = otherHorarios.FirstOrDefault(oh => oh.DiaSemana == h.DiaSemana && (oh.HoraInicio < h.HoraFin && h.HoraInicio < oh.HoraFin));
 if (conflictEntry != null)
 {
 // intentar obtener código del curso en conflicto
 var conflictingCourse = await _ctx.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == conflictEntry.CursoId);
 var code = conflictingCourse?.Codigo ?? conflictEntry.CursoId.ToString();
 return (false, $"Conflicto de horario con curso {code} (día {h.DiaSemana})");
 }
 }
 }
 }
 }

 _ctx.CursoDocentes.Add(new CursoDocente { CursoId = cursoId, DocenteId = docenteId, Activo = true });
 await _ctx.SaveChangesAsync();
 try { await _bitacora.RegistrarAsync(usuarioId, $"Asignar docente {docenteId} a curso {cursoId}", "Academico", ip); } catch { }
 return (true, null);
 }

 public async Task<bool> QuitarDocenteAsync(int cursoId, int docenteId, int usuarioId, string ip)
 {
 var rel = await _ctx.CursoDocentes.FirstOrDefaultAsync(cd => cd.CursoId == cursoId && cd.DocenteId == docenteId);
 if (rel == null) return false;
 _ctx.CursoDocentes.Remove(rel);
 await _ctx.SaveChangesAsync();
 var curso = await _ctx.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cursoId);
 var docente = await _ctx.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == docenteId);
 try { await _bitacora.RegistrarAsync(usuarioId, $"Quitar docente {docente?.Nombre} {docente?.Apellidos} de curso {curso?.Codigo}", "Cursos", ip); } catch { }
 return true;
 }

 public async Task<IEnumerable<DocenteAsignadoDTO>> GetDocentesAsignadosAsync(int cursoId)
 {
 return await _ctx.CursoDocentes
 .Where(cd => cd.CursoId == cursoId)
 .Include(cd => cd.Docente)
 .Select(cd => new DocenteAsignadoDTO { DocenteId = cd.DocenteId, NombreCompleto = cd.Docente.Nombre + " " + cd.Docente.Apellidos, Activo = cd.Activo })
 .ToListAsync();
 }

 public async Task<(bool ok, string? error)> AddHorarioAsync(int cursoId, int diaSemana, TimeSpan inicio, TimeSpan fin, int usuarioId, string ip)
 {
 if (fin <= inicio) return (false, "Rango inválido");
 var curso = await _ctx.Cursos.FirstOrDefaultAsync(c => c.Id == cursoId);
 if (curso == null) return (false, "Curso no encontrado");
 // superposición en mismo curso
 var overlap = await _ctx.HorariosCurso.AnyAsync(h => h.CursoId == cursoId && h.DiaSemana == diaSemana &&
 ((inicio >= h.HoraInicio && inicio < h.HoraFin) || (fin > h.HoraInicio && fin <= h.HoraFin) || (inicio <= h.HoraInicio && fin >= h.HoraFin)));
 if (overlap) return (false, "Conflicto con otro horario del curso");
 _ctx.HorariosCurso.Add(new HorarioCurso { CursoId = cursoId, DiaSemana = diaSemana, HoraInicio = inicio, HoraFin = fin });
 await _ctx.SaveChangesAsync();
 try { await _bitacora.RegistrarAsync(usuarioId, $"Agregar horario {diaSemana} {inicio}-{fin} a {curso.Codigo}", "Cursos", ip); } catch { }
 return (true, null);
 }

 public async Task<bool> RemoveHorarioAsync(int id, int usuarioId, string ip)
 {
 var hor = await _ctx.HorariosCurso.Include(h => h.Curso).FirstOrDefaultAsync(h => h.Id == id);
 if (hor == null) return false;
 _ctx.HorariosCurso.Remove(hor);
 await _ctx.SaveChangesAsync();
 try { await _bitacora.RegistrarAsync(usuarioId, $"Quitar horario {hor.DiaSemana} {hor.HoraInicio}-{hor.HoraFin} de {hor.Curso.Codigo}", "Cursos", ip); } catch { }
 return true;
 }

 public async Task<IEnumerable<object>> GetHorariosAsync(int cursoId)
 {
 return await _ctx.HorariosCurso.Where(h => h.CursoId == cursoId).OrderBy(h => h.DiaSemana).ThenBy(h => h.HoraInicio)
 .Select(h => new { h.Id, h.DiaSemana, HoraInicio = h.HoraInicio.ToString(), HoraFin = h.HoraFin.ToString() })
 .ToListAsync();
 }

 public async Task<IEnumerable<CursoDTO>> GetCursosDeDocenteAsync(int docenteId)
 {
 var cursos = await _ctx.CursoDocentes
 .Where(cd => cd.DocenteId == docenteId)
 .Include(cd => cd.Curso).ThenInclude(c => c.Cuatrimestre)
 .Include(cd => cd.Curso).ThenInclude(c => c.CursoDocentes).ThenInclude(x => x.Docente)
 .Select(cd => cd.Curso)
 .Distinct()
 .OrderBy(c => c.Nombre)
 .ToListAsync();

 return cursos.Select(c => new CursoDTO
 {
 Id = c.Id,
 Codigo = c.Codigo,
 Nombre = c.Nombre,
 Descripcion = c.Descripcion,
 Creditos = c.Creditos,
 Cuatrimestre = c.Cuatrimestre?.Nombre,
 CuatrimestreNumero = c.Cuatrimestre?.Numero,
 Docentes = c.CursoDocentes.Select(x => x.Docente.Nombre + " " + x.Docente.Apellidos).ToList()
 });
 }

 public async Task<IEnumerable<CursoDTO>> GetCursosDisponiblesParaDocenteAsync(int docenteId, int? cuatrimestreId)
 {
 var asignadosIds = await _ctx.CursoDocentes.Where(cd => cd.DocenteId == docenteId).Select(cd => cd.CursoId).ToListAsync();
 var q = _ctx.Cursos.Include(c => c.Cuatrimestre).Include(c => c.CursoDocentes).ThenInclude(x => x.Docente).AsQueryable();
 if (cuatrimestreId.HasValue) q = q.Where(c => c.CuatrimestreId == cuatrimestreId);
 var disponibles = await q.Where(c => !asignadosIds.Contains(c.Id)).OrderBy(c => c.Nombre).ToListAsync();

 return disponibles.Select(c => new CursoDTO
 {
 Id = c.Id,
 Codigo = c.Codigo,
 Nombre = c.Nombre,
 Descripcion = c.Descripcion,
 Creditos = c.Creditos,
 Cuatrimestre = c.Cuatrimestre?.Nombre,
 CuatrimestreNumero = c.Cuatrimestre?.Numero,
 Docentes = c.CursoDocentes.Select(x => x.Docente.Nombre + " " + x.Docente.Apellidos).ToList()
 });
 }
 }
}
