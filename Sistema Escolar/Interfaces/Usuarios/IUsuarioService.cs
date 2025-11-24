using SistemaEscolar.DTOs.Usuarios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaEscolar.Interfaces.Usuarios
{
 // Define operaciones de usuarios
 public interface IUsuarioService
 {
 Task<IEnumerable<UsuarioDTO>> GetAllAsync();
 Task<UsuarioDTO> GetByIdAsync(int id);
 Task<(bool ok, string? error)> CreateAsync(UsuarioCreateDTO dto);
 Task<(bool ok, string? error)> UpdateAsync(int id, UsuarioUpdateDTO dto);
 Task<(bool ok, string? error)> DeleteAsync(int id);
 }
}
