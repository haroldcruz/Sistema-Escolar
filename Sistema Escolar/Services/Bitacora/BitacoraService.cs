using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Bitacora;
using SistemaEscolar.Interfaces.Bitacora;
using SistemaEscolar.Models.Bitacora;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace SistemaEscolar.Services.Bitacora
{
    // Implementación de bitácora
    public class BitacoraService : IBitacoraService
    {
        private readonly ApplicationDbContext _context;

        public BitacoraService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarAsync(int usuarioId, string accion, string modulo, string ip)
        {
            var entry = new BitacoraEntry
            {
                UsuarioId = usuarioId,
                Accion = accion,
                Modulo = modulo,
                Ip = ip,
                Fecha = DateTime.UtcNow
            };

            _context.BitacoraEntries.Add(entry);
            await _context.SaveChangesAsync();
        }

        public async Task RegistrarLoginAsync(int usuarioId, string ip)
            => await RegistrarAsync(usuarioId, "Login", "Autenticacion", ip);

        public async Task<IEnumerable<BitacoraDTO>> GetAllAsync()
        {
            var entities = await _context.BitacoraEntries.Include(b => b.Usuario)
                .OrderByDescending(b => b.Fecha)
                .ToListAsync();
            return entities.Select(MapDto).ToList();
        }

        public async Task<IEnumerable<BitacoraDTO>> GetPagedAsync(int page, int pageSize, string? usuario, string? modulo, string? accion)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var query = _context.BitacoraEntries.Include(b => b.Usuario).AsQueryable();
            if (!string.IsNullOrWhiteSpace(usuario)) query = query.Where(b => (b.Usuario.Nombre + " " + b.Usuario.Apellidos).Contains(usuario));
            if (!string.IsNullOrWhiteSpace(modulo)) query = query.Where(b => b.Modulo.Contains(modulo));
            if (!string.IsNullOrWhiteSpace(accion)) query = query.Where(b => b.Accion.Contains(accion));
            var entities = await query.OrderByDescending(b => b.Fecha)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync();
            return entities.Select(MapDto).ToList();
        }

        private static BitacoraDTO MapDto(BitacoraEntry b) => new BitacoraDTO
        {
            Id = b.Id,
            Usuario = b.Usuario.Nombre + " " + b.Usuario.Apellidos,
            Accion = b.Accion,
            Modulo = b.Modulo,
            Ip = b.Ip,
            Fecha = b.Fecha.ToString("yyyy-MM-dd HH:mm")
        };
    }
}
