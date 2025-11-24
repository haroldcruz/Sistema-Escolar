using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Usuarios;
using SistemaEscolar.Interfaces.Bitacora;
using SistemaEscolar.Interfaces.Usuarios;
using SistemaEscolar.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SistemaEscolar.Services.Usuarios
{
 public class UsuarioService : IUsuarioService
 {
 private readonly ApplicationDbContext _ctx;
 private readonly IBitacoraService _bitacora;
 private readonly IHttpContextAccessor _http;

 public UsuarioService(ApplicationDbContext ctx, IBitacoraService bitacora, IHttpContextAccessor http)
 {
 _ctx = ctx;
 _bitacora = bitacora;
 _http = http;
 }

 private int CurrentUserId()
 {
 try
 {
 var val = _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
 if (int.TryParse(val, out var id)) return id;
 }
 catch { }
 return0;
 }
 private string Ip() => _http.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";

 public async Task<IEnumerable<DTOs.Usuarios.UsuarioDTO>> GetAllAsync()
 {
 var users = await _ctx.Usuarios.Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol).ToListAsync();
 return users.Select(u => new DTOs.Usuarios.UsuarioDTO
 {
 Id = u.Id,
 NombreCompleto = (u.Nombre + " " + u.Apellidos).Trim(),
 Email = u.Email,
 Identificacion = u.Identificacion,
 Roles = u.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList()
 }).ToList();
 }

 public async Task<DTOs.Usuarios.UsuarioDTO> GetByIdAsync(int id)
 {
 var u = await _ctx.Usuarios.Include(us => us.UsuarioRoles).ThenInclude(ur => ur.Rol).FirstOrDefaultAsync(x => x.Id == id);
 if (u == null) return null!;
 return new DTOs.Usuarios.UsuarioDTO
 {
 Id = u.Id,
 NombreCompleto = (u.Nombre + " " + u.Apellidos).Trim(),
 Email = u.Email,
 Identificacion = u.Identificacion,
 Roles = u.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList()
 };
 }

 public async Task<(bool ok, string? error)> CreateAsync(UsuarioCreateDTO dto)
 {
 // Validaciones básicas
 if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Identificacion)) return (false, "Email e identificación son obligatorios");
 // Duplicados
 if (await _ctx.Usuarios.AnyAsync(x => x.Email == dto.Email)) return (false, "Email duplicado");
 if (await _ctx.Usuarios.AnyAsync(x => x.Identificacion == dto.Identificacion)) return (false, "Identificación duplicada");

 try
 {
 var user = new Usuario
 {
 Nombre = dto.Nombre,
 Apellidos = dto.Apellidos,
 Email = dto.Email,
 Identificacion = dto.Identificacion,
 IsActive = true
 };

 // crear hash y salt
 SistemaEscolar.Helpers.PasswordHasher.CreatePasswordHash(dto.Password ?? string.Empty, out var hash, out var salt);
 user.PasswordHash = hash;
 user.PasswordSalt = salt;

 _ctx.Usuarios.Add(user);
 // preparar roles
 foreach (var rid in dto.RolesIds.Distinct())
 {
 if (await _ctx.Roles.AnyAsync(r => r.Id == rid))
 {
 _ctx.UsuarioRoles.Add(new UsuarioRol { Usuario = user, RolId = rid });
 }
 }

 // Guardar todo en una sola transacción implícita por SaveChanges
 await _ctx.SaveChangesAsync();

 // Registrar en bitácora (usuario creador desconocido aquí) pero no propagar errores de bitácora
 try
 {
 await _bitacora.RegistrarAsync(CurrentUserId(), $"Crear usuario {user.Email}", "Seguridad", Ip());
 }
 catch
 {
 // swallow bitacora errors
 }

 return (true, null);
 }
 catch (DbUpdateException ex)
 {
 // Registrar error y devolver false con detalle para controlador, but swallow bitacora errors
 try
 {
 await _bitacora.RegistrarAsync(CurrentUserId(), $"Error al crear usuario: {ex.InnerException?.Message ?? ex.Message}", "Seguridad", Ip());
 }
 catch { }
 return (false, "Error al guardar en la base de datos");
 }
 }

 public async Task<(bool ok, string? error)> UpdateAsync(int id, UsuarioUpdateDTO dto)
 {
 var user = await _ctx.Usuarios.Include(u => u.UsuarioRoles).FirstOrDefaultAsync(u => u.Id == id);
 if (user == null) return (false, "Usuario no encontrado");
 // Validar duplicados (excluyendo al propio usuario)
 if (await _ctx.Usuarios.AnyAsync(x => x.Email == dto.Email && x.Id != id)) return (false, "Email duplicado");
 if (await _ctx.Usuarios.AnyAsync(x => x.Identificacion == dto.Identificacion && x.Id != id)) return (false, "Identificación duplicada");

 try
 {
 user.Nombre = dto.Nombre;
 user.Apellidos = dto.Apellidos;
 user.Email = dto.Email;
 user.Identificacion = dto.Identificacion;
 user.IsActive = dto.IsActive;

 var newPassword = dto.Password ?? dto.NewPassword;
 if (!string.IsNullOrWhiteSpace(newPassword))
 {
 SistemaEscolar.Helpers.PasswordHasher.CreatePasswordHash(newPassword, out var hash, out var salt);
 user.PasswordHash = hash;
 user.PasswordSalt = salt;
 }

 // actualizar roles: eliminar no seleccionados, agregar nuevos
 var current = user.UsuarioRoles.Select(ur => ur.RolId).ToList();
 var toRemove = user.UsuarioRoles.Where(ur => !dto.RolesIds.Contains(ur.RolId)).ToList();
 _ctx.UsuarioRoles.RemoveRange(toRemove);
 var toAdd = dto.RolesIds.Where(rid => !current.Contains(rid)).Distinct();
 foreach (var rid in toAdd)
 {
 if (await _ctx.Roles.AnyAsync(r => r.Id == rid)) _ctx.UsuarioRoles.Add(new UsuarioRol { UsuarioId = user.Id, RolId = rid });
 }

 // Guardar todos los cambios en una sola operación
 await _ctx.SaveChangesAsync();

 // Registrar en bitácora pero swallow any bitacora exceptions
 try
 {
 await _bitacora.RegistrarAsync(CurrentUserId(), $"Actualizar usuario {user.Email}. Perms added/removed", "Seguridad", Ip());
 }
 catch { }

 var added = string.Join(',', toAdd);
 var removed = string.Join(',', toRemove.Select(x => x.RolId));
 return (true, null);
 }
 catch (DbUpdateException ex)
 {
 try { await _bitacora.RegistrarAsync(CurrentUserId(), $"Error al actualizar usuario: {ex.InnerException?.Message ?? ex.Message}", "Seguridad", Ip()); } catch { }
 return (false, "Error al guardar en la base de datos");
 }
 }

 public async Task<(bool ok, string? error)> DeleteAsync(int id)
 {
 var user = await _ctx.Usuarios.Include(u => u.UsuarioRoles).FirstOrDefaultAsync(idu => idu.Id == id);
 if (user == null) return (false, "Usuario no encontrado");

 try
 {
 // Soft delete: desactivar
 user.IsActive = false;
 await _ctx.SaveChangesAsync();
 try { await _bitacora.RegistrarAsync(CurrentUserId(), $"Desactivar usuario {user.Email}", "Seguridad", Ip()); } catch { }
 return (true, null);
 }
 catch (DbUpdateException ex)
 {
 try { await _bitacora.RegistrarAsync(CurrentUserId(), $"Error al desactivar usuario: {ex.InnerException?.Message ?? ex.Message}", "Seguridad", Ip()); } catch { }
 return (false, "Error al guardar en la base de datos");
 }
 }
 }
}
