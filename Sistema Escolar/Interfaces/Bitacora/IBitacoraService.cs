using SistemaEscolar.DTOs.Bitacora;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SistemaEscolar.Interfaces.Bitacora
{
 // Operaciones de auditoría
 public interface IBitacoraService
 {
 Task RegistrarAsync(int usuarioId, string accion, string modulo, string ip);
 Task<IEnumerable<BitacoraDTO>> GetAllAsync();
 Task<IEnumerable<BitacoraDTO>> GetPagedAsync(int page, int pageSize, string? usuario, string? modulo, string? accion); // ya existente
 Task RegistrarLoginAsync(int usuarioId, string ip); // nuevo para logins
 }
}
