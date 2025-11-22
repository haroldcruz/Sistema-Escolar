using System.Collections.Generic;using System.Threading.Tasks;using SistemaEscolar.DTOs.Usuarios;
namespace SistemaEscolar.Interfaces.Usuarios
{
 public interface IRolService
 {
 Task<IEnumerable<RolDTO>> GetAllAsync();
 Task<RolDTO?> GetByIdAsync(int id);
 Task<bool> CreateAsync(RolCreateDTO dto, int usuarioId, string ip);
 Task<bool> UpdateAsync(RolUpdateDTO dto, int usuarioId, string ip);
 Task<bool> DeleteAsync(int id, int usuarioId, string ip);
 }
}
