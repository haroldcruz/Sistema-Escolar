using System.Collections.Generic;

namespace SistemaEscolar.DTOs.Usuarios
{
    // Datos para actualizar un usuario
    public class UsuarioUpdateDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public List<int> RolesIds { get; set; } = new();
        // NUEVO: contraseña nueva opcional
        public string? NewPassword { get; set; }
    }
}
