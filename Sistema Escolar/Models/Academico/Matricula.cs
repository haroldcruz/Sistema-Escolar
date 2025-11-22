using System;
using System.Collections.Generic;

namespace SistemaEscolar.Models.Academico
{
 // Matrícula del estudiante en un curso
 public class Matricula
 {
 public int Id { get; set; } // PK

 // IDs y navegación ahora opcionales (nullable) según requerimiento
 public int? EstudianteId { get; set; }
 public Usuario? Estudiante { get; set; } // Usa Usuario como estudiante

 public int? CursoId { get; set; }
 public Curso? Curso { get; set; }

 public int? CuatrimestreId { get; set; }
 public Cuatrimestre? Cuatrimestre { get; set; }

 public DateTime FechaMatricula { get; set; }

 public ICollection<Evaluacion> Evaluaciones { get; set; } = new List<Evaluacion>();
 }
}
