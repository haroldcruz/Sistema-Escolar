using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Usuarios;
using SistemaEscolar.Interfaces.Usuarios;
using SistemaEscolar.Models;
using SistemaEscolar.Helpers;
using System.Linq;
using SistemaEscolar.Interfaces.Bitacora;

namespace SistemaEscolar.Services.Usuarios
{
 // Implementación del servicio de usuarios
 public class UsuarioService : IUsuarioService
 {
 private readonly ApplicationDbContext _context;
 private readonly IBitacoraService _bitacora;

 public UsuarioService(ApplicationDbContext context, IBitacoraService bitacora)
 {
 _context = context;
 _bitacora = bitacora;
 }

 // Lista de usuarios
 public async Task<IEnumerable<UsuarioDTO>> GetAllAsync()
 {
 return await _context.Usuarios
 .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
 .Select(u => new UsuarioDTO
 {
 Id = u.Id,
 NombreCompleto = $"{u.Nombre} {u.Apellidos}",
 Email = u.Email,
 Identificacion = u.Identificacion,
 Roles = u.UsuarioRoles.Select(r => r.Rol.Nombre).ToList()
 })
 .ToListAsync();
 }

 // Usuario por Id
 public async Task<UsuarioDTO> GetByIdAsync(int id)
 {
 var u = await _context.Usuarios
 .Include(x => x.UsuarioRoles).ThenInclude(r => r.Rol)
 .FirstOrDefaultAsync(x => x.Id == id);

 if (u == null)
 return null!;

 return new UsuarioDTO
 {
 Id = u.Id,
 NombreCompleto = $"{u.Nombre} {u.Apellidos}",
 Email = u.Email,
 Identificacion = u.Identificacion,
 Roles = u.UsuarioRoles.Select(r => r.Rol.Nombre).ToList()
 };
 }

 // Crear usuario
 public async Task<bool> CreateAsync(UsuarioCreateDTO dto)
 {
 PasswordHasher.CreatePasswordHash(dto.Password, out var hash, out var salt);

 var usuario = new Usuario
 {
 Nombre = dto.Nombre,
 Apellidos = dto.Apellidos,
 Identificacion = dto.Identificacion,
 Email = dto.Email,
 PasswordHash = hash,
 PasswordSalt = salt,
 Activo = true,
 FechaCreacion = DateTime.UtcNow
 };

 _context.Add(usuario);
 await _context.SaveChangesAsync();

 // Asignar roles
 foreach (var rolId in dto.RolesIds)
 {
 _context.UsuarioRoles.Add(new UsuarioRol
 {
 UsuarioId = usuario.Id,
 RolId = rolId
 });
 }

 await _context.SaveChangesAsync();

 // Bitácora: creación con roles
 var rolesStr = string.Join(",", dto.RolesIds);
 await _bitacora.RegistrarAsync(usuario.Id, $"Asignación inicial roles [{rolesStr}]", "Usuarios", "0.0.0.0");

 return true;
 }

 // Actualizar usuario
 public async Task<bool> UpdateAsync(int id, UsuarioUpdateDTO dto)
 {
 var usuario = await _context.Usuarios.FindAsync(id);
 if (usuario == null)
 return false;

 usuario.Nombre = dto.Nombre;
 usuario.Apellidos = dto.Apellidos;
 usuario.Email = dto.Email;
 usuario.Identificacion = dto.Identificacion;

 // Roles actuales para bitácora
 var prevRoles = await _context.UsuarioRoles.Where(r => r.UsuarioId == id).Select(r => r.RolId).ToListAsync();

 // Remover roles previos
 var roles = _context.UsuarioRoles.Where(r => r.UsuarioId == id);
 _context.UsuarioRoles.RemoveRange(roles);

 // Guardar nuevos roles
 foreach (var rolId in dto.RolesIds)
 _context.UsuarioRoles.Add(new UsuarioRol { UsuarioId = id, RolId = rolId });

 await _context.SaveChangesAsync();

 // Bitácora diferencias
 var nuevos = dto.RolesIds.Except(prevRoles);
 var removidos = prevRoles.Except(dto.RolesIds);
 var cambio = $"Roles añadidos: [{string.Join(',', nuevos)}]; Roles removidos: [{string.Join(',', removidos)}]";
 await _bitacora.RegistrarAsync(id, cambio, "Usuarios", "0.0.0.0");

 return true;
 }

 public async Task<bool> DeleteAsync(int id)
 {
 var usuario = await _context.Usuarios.FindAsync(id);
 if (usuario == null)
 return false;

 _context.Usuarios.Remove(usuario);
 await _context.SaveChangesAsync();
 await _bitacora.RegistrarAsync(id, "Eliminación de usuario", "Usuarios", "0.0.0.0");
 return true;
 }
 }
}
