using System;
using System.Collections.Generic;

namespace SistemaEscolar.Models.Academico
{
    // Información del curso
    public class Curso
    {
        public int Id { get; set; } // PK
        public required string Codigo { get; set; }
        public required string Nombre { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int Creditos { get; set; }

        // FK ahora nullable (curso puede existir sin asignar cuatrimestre inicialmente)
        public int? CuatrimestreId { get; set; } // FK
        public Cuatrimestre? Cuatrimestre { get; set; }

        public int UsuarioCreacion { get; set; }
        public DateTime FechaCreacion { get; set; }

        public int? UsuarioModificacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // Relación con matrículas
        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

        // Docentes asignados
        public ICollection<CursoDocente> CursoDocentes { get; set; } = new List<CursoDocente>();
    }
}
