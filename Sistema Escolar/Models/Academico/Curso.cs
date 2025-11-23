using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaEscolar.Models.Academico
{
    // Información del curso
    public class Curso
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0, 50)]
        public int Creditos { get; set; }

        // FK ahora nullable (curso puede existir sin asignar cuatrimestre inicialmente)
        public int? CuatrimestreId { get; set; } // FK
        public Cuatrimestre? Cuatrimestre { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; }
        public int? CreadoPorId { get; set; }
        public Usuario? CreadoPor { get; set; }

        // Relación con matrículas
        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

        // Docentes asignados
        public ICollection<CursoDocente> CursoDocentes { get; set; } = new List<CursoDocente>();
    }
}
