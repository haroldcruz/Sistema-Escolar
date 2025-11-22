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
 Task<bool> CreateAsync(UsuarioCreateDTO dto);
 Task<bool> UpdateAsync(int id, UsuarioUpdateDTO dto);
 Task<bool> DeleteAsync(int id);
 }
}
