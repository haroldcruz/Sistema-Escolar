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
 }
}
