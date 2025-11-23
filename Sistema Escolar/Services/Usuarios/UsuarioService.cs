using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Usuarios;
using SistemaEscolar.Interfaces.Usuarios;
using SistemaEscolar.Interfaces.Bitacora;
using SistemaEscolar.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SistemaEscolar.Services.Usuarios
{
 public class UsuarioService : IUsuarioService
 {
 private readonly ApplicationDbContext _ctx;
 private readonly IBitacoraService _bitacora;

 public UsuarioService(ApplicationDbContext ctx, IBitacoraService bitacora)
 {
 _ctx = ctx;
 _bitacora = bitacora;
 }

 public async Task<IEnumerable<UsuarioDTO>> GetAllAsync()
 {
 var users = await _ctx.Usuarios
 .Include(u => u.UsuarioRoles)
 .ThenInclude(ur => ur.Rol)
 .AsNoTracking()
 .ToListAsync();

 return users.Select(u => new UsuarioDTO
 {
 Id = u.Id,
 NombreCompleto = (u.Nombre + " " + u.Apellidos).Trim(),
 Email = u.Email,
 Identificacion = u.Identificacion ?? string.Empty,
 Roles = u.UsuarioRoles?.Select(x => x.Rol?.Nombre ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>()
 }).ToList();
 }

 public async Task<UsuarioDTO> GetByIdAsync(int id)
 {
 var u = await _ctx.Usuarios
 .Include(x => x.UsuarioRoles).ThenInclude(ur => ur.Rol)
 .AsNoTracking()
 .FirstOrDefaultAsync(x => x.Id == id);
 if (u == null) return null!;

 return new UsuarioDTO
 {
 Id = u.Id,
 NombreCompleto = (u.Nombre + " " + u.Apellidos).Trim(),
 Email = u.Email,
 Identificacion = u.Identificacion ?? string.Empty,
 Roles = u.UsuarioRoles?.Select(x => x.Rol?.Nombre ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>()
 };
 }

 public async Task<bool> CreateAsync(UsuarioCreateDTO dto)
 {
 if (dto == null) return false;
 if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Nombre)) return false;

 // Evitar duplicados por email
 if (await _ctx.Usuarios.AnyAsync(u => u.Email == dto.Email)) return false;

 var user = new Usuario
 {
 Nombre = dto.Nombre?.Trim() ?? string.Empty,
 Apellidos = dto.Apellidos?.Trim() ?? string.Empty,
 Email = dto.Email.Trim(),
 Identificacion = dto.Identificacion?.Trim() ?? string.Empty,
 };

 _ctx.Usuarios.Add(user);
 await _ctx.SaveChangesAsync();

 // Asignar roles si vienen
 if (dto.RolesIds != null && dto.RolesIds.Any())
 {
 foreach (var rid in dto.RolesIds.Distinct())
 {
 if (await _ctx.Roles.AnyAsync(r => r.Id == rid))
 {
 _ctx.UsuarioRoles.Add(new UsuarioRol { UsuarioId = user.Id, RolId = rid });
 }
 }
 await _ctx.SaveChangesAsync();
 }

 // Registrar en bitácora (usuarioId desconocido desde aquí)
 try
 {
 await _bitacora.RegistrarAsync(0, $"Crear usuario {user.Email}", "Usuarios", string.Empty);
 }
 catch { }

 return true;
 }

 public async Task<bool> UpdateAsync(int id, UsuarioUpdateDTO dto)
 {
 if (dto == null) return false;
 var user = await _ctx.Usuarios.Include(u => u.UsuarioRoles).FirstOrDefaultAsync(u => u.Id == id);
 if (user == null) return false;

 // Actualizar datos
 user.Nombre = dto.Nombre?.Trim() ?? user.Nombre;
 user.Apellidos = dto.Apellidos?.Trim() ?? user.Apellidos;
 user.Email = dto.Email?.Trim() ?? user.Email;
 user.Identificacion = dto.Identificacion?.Trim() ?? user.Identificacion;

 // Actualizar roles: sincronizar
 var prev = user.UsuarioRoles.Select(x => x.RolId).ToList();
 var incoming = dto.RolesIds ?? new List<int>();

 var toRemove = user.UsuarioRoles.Where(ur => !incoming.Contains(ur.RolId)).ToList();
 if (toRemove.Any()) _ctx.UsuarioRoles.RemoveRange(toRemove);

 var toAdd = incoming.Where(rid => !prev.Contains(rid)).Distinct();
 foreach (var rid in toAdd)
 {
 if (await _ctx.Roles.AnyAsync(r => r.Id == rid))
 _ctx.UsuarioRoles.Add(new UsuarioRol { UsuarioId = user.Id, RolId = rid });
 }

 await _ctx.SaveChangesAsync();

 // Bitácora
 try
 {
 await _bitacora.RegistrarAsync(0, $"Actualizar usuario {user.Email}", "Usuarios", string.Empty);
 }
 catch { }

 return true;
 }

 public async Task<bool> DeleteAsync(int id)
 {
 var user = await _ctx.Usuarios.Include(u => u.UsuarioRoles).FirstOrDefaultAsync(u => u.Id == id);
 if (user == null) return false;

 // No eliminar si tiene matrículas o asignaciones de docente
 var hasMatriculas = await _ctx.Matriculas.AnyAsync(m => m.EstudianteId == id);
 var hasAsignaciones = await _ctx.CursoDocentes.AnyAsync(cd => cd.DocenteId == id);
 if (hasMatriculas || hasAsignaciones) return false;

 _ctx.UsuarioRoles.RemoveRange(user.UsuarioRoles);
 _ctx.Usuarios.Remove(user);
 await _ctx.SaveChangesAsync();

 try { await _bitacora.RegistrarAsync(0, $"Eliminar usuario {user.Email}", "Usuarios", string.Empty); } catch { }

 return true;
 }
 }
}
