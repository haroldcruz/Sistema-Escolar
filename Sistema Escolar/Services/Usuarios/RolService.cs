using Microsoft.EntityFrameworkCore;using SistemaEscolar.Data;using SistemaEscolar.DTOs.Usuarios;using SistemaEscolar.Interfaces.Usuarios;using SistemaEscolar.Models;using SistemaEscolar.Interfaces.Bitacora;using System.Collections.Generic;using System.Linq;using System.Threading.Tasks;using System;
namespace SistemaEscolar.Services.Usuarios
{
 public class RolService : IRolService
 {
 private readonly ApplicationDbContext _ctx; private readonly IBitacoraService _bitacora;
 public RolService(ApplicationDbContext ctx, IBitacoraService bitacora){ _ctx = ctx; _bitacora = bitacora; }
 public async Task<IEnumerable<RolDTO>> GetAllAsync(){
 var roles = await _ctx.Roles.Include(r=>r.RolPermisos).ThenInclude(rp=>rp.Permiso).Include(r=>r.UsuarioRoles).ToListAsync();
 return roles.Select(r=> new RolDTO{ Id=r.Id, Nombre=r.Nombre, Permisos = r.RolPermisos.Select(p=>p.Permiso.Codigo).ToList(), UsuariosCount = r.UsuarioRoles.Count }).ToList(); }
 public async Task<RolDTO?> GetByIdAsync(int id){
 var r = await _ctx.Roles.Include(x=>x.RolPermisos).ThenInclude(p=>p.Permiso).Include(x=>x.UsuarioRoles).FirstOrDefaultAsync(x=>x.Id==id); if(r==null) return null; return new RolDTO{ Id=r.Id, Nombre=r.Nombre, Permisos = r.RolPermisos.Select(p=>p.Permiso.Codigo).ToList(), UsuariosCount = r.UsuarioRoles.Count }; }
 public async Task<bool> CreateAsync(RolCreateDTO dto, int usuarioId, string ip){ if(string.IsNullOrWhiteSpace(dto.Nombre)) return false; if(await _ctx.Roles.AnyAsync(r=>r.Nombre==dto.Nombre)) return false; var rol = new Rol{ Nombre=dto.Nombre }; _ctx.Roles.Add(rol); await _ctx.SaveChangesAsync(); foreach(var pid in dto.PermisosIds.Distinct()){ if(await _ctx.Permisos.AnyAsync(p=>p.Id==pid)) _ctx.RolPermisos.Add(new RolPermiso{ RolId=rol.Id, PermisoId=pid }); }
 await _ctx.SaveChangesAsync(); await _bitacora.RegistrarAsync(usuarioId,$"Crear rol {rol.Nombre}","Seguridad",ip); return true; }
 public async Task<bool> UpdateAsync(RolUpdateDTO dto, int usuarioId, string ip){ var rol = await _ctx.Roles.Include(r=>r.RolPermisos).FirstOrDefaultAsync(r=>r.Id==dto.Id); if(rol==null) return false; var prevPerms = rol.RolPermisos.Select(p=>p.PermisoId).ToList(); rol.Nombre = dto.Nombre; // actualizar permisos
 var toRemove = rol.RolPermisos.Where(rp=> !dto.PermisosIds.Contains(rp.PermisoId)).ToList(); _ctx.RolPermisos.RemoveRange(toRemove);
 var toAdd = dto.PermisosIds.Where(pid=> !prevPerms.Contains(pid)); foreach(var pid in toAdd) if(await _ctx.Permisos.AnyAsync(p=>p.Id==pid)) _ctx.RolPermisos.Add(new RolPermiso{ RolId=rol.Id, PermisoId=pid }); await _ctx.SaveChangesAsync(); var added = string.Join(',', toAdd); var removed = string.Join(',', toRemove.Select(x=>x.PermisoId)); await _bitacora.RegistrarAsync(usuarioId,$"Actualizar rol {rol.Nombre}. Perms added:[{added}] removed:[{removed}]","Seguridad",ip); return true; }
 public async Task<bool> DeleteAsync(int id, int usuarioId, string ip){ var rol = await _ctx.Roles.Include(r=>r.UsuarioRoles).FirstOrDefaultAsync(r=>r.Id==id); if(rol==null) return false; if(rol.UsuarioRoles.Any()) return false; _ctx.Roles.Remove(rol); await _ctx.SaveChangesAsync(); await _bitacora.RegistrarAsync(usuarioId,$"Eliminar rol {rol.Nombre}","Seguridad",ip); return true; }
 }
}
