using System;

namespace SistemaEscolar.Models.Academico
{
 // Evaluación del estudiante
 public class Evaluacion
 {
 public int Id { get; set; } // PK
        public decimal Nota { get; set; } // nota con decimales

        public string Observaciones { get; set; } = string.Empty;
 public string Participacion { get; set; } = string.Empty;
 public string Estado { get; set; } = string.Empty; // Aprobado o Reprobado

 public int MatriculaId { get; set; } // FK
 public Matricula? Matricula { get; set; } // cambiado a nullable para seed

 public DateTime FechaRegistro { get; set; }
 public int UsuarioRegistro { get; set; }
 }
}
