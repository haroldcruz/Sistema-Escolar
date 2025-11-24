using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Historial;
using SistemaEscolar.Interfaces.Historial;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace SistemaEscolar.Services.Historial
{
 public class HistorialService : IHistorialService
 {
 private readonly ApplicationDbContext _context;
 public HistorialService(ApplicationDbContext context) { _context = context; }

 public async Task<EstudianteHistorialDTO> GetHistorialAsync(int estudianteId)
 {
 var estudiante = await _context.Usuarios.FindAsync(estudianteId);
 if (estudiante == null)
 return new EstudianteHistorialDTO { EstudianteId = estudianteId, NombreCompleto = string.Empty };

 var evaluaciones = await _context.Evaluaciones
 .Include(e => e.Matricula).ThenInclude(m => m.Curso)
 .Include(e => e.Matricula).ThenInclude(m => m.Cuatrimestre)
 .Where(e => e.Matricula != null && e.Matricula.EstudianteId == estudianteId)
 .ToListAsync();

 var registros = evaluaciones.Select(e => new HistorialItemDTO
 {
 Curso = e.Matricula!.Curso?.Nombre ?? string.Empty,
 Cuatrimestre = e.Matricula!.Cuatrimestre?.Nombre ?? string.Empty,
 Nota = e.Nota,
 Estado = e.Estado,
 Participacion = e.Participacion,
 Observaciones = e.Observaciones,
 Fecha = e.FechaRegistro.ToShortDateString()
 }).ToList();

 return new EstudianteHistorialDTO
 {
 EstudianteId = estudiante.Id,
 NombreCompleto = $"{estudiante.Nombre} {estudiante.Apellidos}",
 Registros = registros
 };
 }

 public async Task<IEnumerable<EstudianteBusquedaDTO>> BuscarEstudiantesAsync(string termino)
 {
 termino = termino?.Trim() ?? string.Empty;
 if (string.IsNullOrEmpty(termino)) return Enumerable.Empty<EstudianteBusquedaDTO>();
 return await _context.Usuarios
 .Where(u => u.Nombre.Contains(termino) || u.Apellidos.Contains(termino) || u.Identificacion.Contains(termino) || u.Email.Contains(termino))
 .OrderBy(u => u.Nombre)
 .Take(20)
 .Select(u => new EstudianteBusquedaDTO
 {
 Id = u.Id,
 NombreCompleto = u.Nombre + " " + u.Apellidos,
 Identificacion = u.Identificacion,
 Email = u.Email
 })
 .ToListAsync();
 }

 public async Task<EstudianteHistorialAgrupadoDTO> GetHistorialAgrupadoAsync(int estudianteId)
 {
 var estudiante = await _context.Usuarios.FindAsync(estudianteId);
 if (estudiante == null) return new EstudianteHistorialAgrupadoDTO { EstudianteId = estudianteId };
 var evaluaciones = await _context.Evaluaciones
 .Include(e => e.Matricula).ThenInclude(m => m.Curso)
 .Include(e => e.Matricula).ThenInclude(m => m.Cuatrimestre)
 .Where(e => e.Matricula != null && e.Matricula.EstudianteId == estudianteId)
 .ToListAsync();
 return MapearAgrupado(estudiante, evaluaciones);
 }

 public async Task<EstudianteHistorialAgrupadoDTO> GetHistorialAgrupadoFiltradoAsync(int estudianteId, DateTime? from, DateTime? to, IEnumerable<string>? cursos)
 {
 var estudiante = await _context.Usuarios.FindAsync(estudianteId);
 if (estudiante == null) return new EstudianteHistorialAgrupadoDTO { EstudianteId = estudianteId };
 var query = _context.Evaluaciones
 .Include(e => e.Matricula).ThenInclude(m => m.Curso)
 .Include(e => e.Matricula).ThenInclude(m => m.Cuatrimestre)
 .Where(e => e.Matricula != null && e.Matricula.EstudianteId == estudianteId);
 if (from.HasValue) query = query.Where(e => e.FechaRegistro >= from.Value);
 if (to.HasValue) query = query.Where(e => e.FechaRegistro <= to.Value);
 if (cursos != null && cursos.Any()) query = query.Where(e => cursos.Contains(e.Matricula!.Curso!.Codigo));
 var evaluaciones = await query.ToListAsync();
 return MapearAgrupado(estudiante, evaluaciones);
 }

 private EstudianteHistorialAgrupadoDTO MapearAgrupado(Models.Usuario estudiante, List<Models.Academico.Evaluacion> evaluaciones)
 {
 var grupos = evaluaciones
 .GroupBy(e => e.Matricula?.Cuatrimestre?.Nombre ?? string.Empty)
 .Select(g => new HistorialCuatrimestreDTO
 {
 Cuatrimestre = g.Key,
 Cursos = g.Select(e => new HistorialItemDTO
 {
 Curso = e.Matricula!.Curso?.Nombre ?? string.Empty,
 Cuatrimestre = e.Matricula!.Cuatrimestre?.Nombre ?? string.Empty,
 Nota = e.Nota,
 Estado = e.Estado,
 Participacion = e.Participacion,
 Observaciones = e.Observaciones,
 Fecha = e.FechaRegistro.ToShortDateString()
 }).OrderBy(x => x.Curso).ToList(),
 Promedio = g.Any() ? g.Average(x => x.Nota) : null
 })
 .OrderBy(x => x.Cuatrimestre)
 .ToList();

 return new EstudianteHistorialAgrupadoDTO
 {
 EstudianteId = estudiante.Id,
 NombreCompleto = estudiante.Nombre + " " + estudiante.Apellidos,
 Cuatrimestres = grupos,
 PromedioGeneral = grupos.SelectMany(c => c.Cursos).Any() ? grupos.SelectMany(c => c.Cursos).Average(c => c.Nota) : null
 };
 }
 }
}
