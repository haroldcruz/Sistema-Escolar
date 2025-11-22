using SistemaEscolar.DTOs.Cursos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaEscolar.Interfaces.Cursos
{
 // Define operaciones de cursos
 public interface ICursoService
 {
 Task<IEnumerable<CursoDTO>> GetAllAsync();
 Task<CursoDTO> GetByIdAsync(int id);
 Task<bool> CreateAsync(CursoCreateDTO dto);
 Task<bool> UpdateAsync(int id, CursoUpdateDTO dto);
 Task<bool> DeleteAsync(int id);

 Task<bool> AsignarDocenteAsync(CursoDocenteDTO dto);
 Task<bool> RemoverDocenteAsync(int cursoId, int docenteId);
 Task<bool> PuedeEliminarAsync(int cursoId); // verifica sin matrículas
 }
}
