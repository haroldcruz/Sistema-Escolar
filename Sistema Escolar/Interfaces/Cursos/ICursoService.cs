using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaEscolar.DTOs.Cursos;

namespace SistemaEscolar.Interfaces.Cursos
{
 public interface ICursoService
 {
 Task<IEnumerable<CursoDTO>> GetAllAsync();
 Task<CursoDTO?> GetByIdAsync(int id);
 Task<bool> CreateAsync(CursoCreateDTO dto, int usuarioId, string ip);
 Task<bool> UpdateAsync(int id, CursoUpdateDTO dto, int usuarioId, string ip);
 Task<bool> DeleteAsync(int id, int usuarioId, string ip);
 Task<(bool ok, string? error)> AsignarDocenteAsync(int cursoId, int docenteId, int usuarioId, string ip);
 Task<bool> QuitarDocenteAsync(int cursoId, int docenteId, int usuarioId, string ip);
 Task<IEnumerable<DocenteAsignadoDTO>> GetDocentesAsignadosAsync(int cursoId);
 // Horarios
 Task<(bool ok, string? error)> AddHorarioAsync(int cursoId, int diaSemana, TimeSpan inicio, TimeSpan fin, int usuarioId, string ip);
 Task<bool> RemoveHorarioAsync(int horarioId, int usuarioId, string ip);
 Task<IEnumerable<object>> GetHorariosAsync(int cursoId);
 // Docente-centricas
 Task<IEnumerable<CursoDTO>> GetCursosDeDocenteAsync(int docenteId);
 Task<IEnumerable<CursoDTO>> GetCursosDisponiblesParaDocenteAsync(int docenteId, int? cuatrimestreId);
 }
}
