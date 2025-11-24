using System;
using System.Collections.Generic;

namespace SistemaEscolar.Models.Academico
{
 public class Matricula
 {
 public int Id { get; set; }

 // Oferta/grupo (nueva)
 public int? CursoOfertaId { get; set; }

 // Compatibilidad: referencia directa a Curso y Cuatrimestre
 public int CursoId { get; set; }
 public int CuatrimestreId { get; set; }

 public int EstudianteId { get; set; }
 public DateTime FechaMatricula { get; set; }
 public bool Activo { get; set; } = true;

 // Navegaciones
 public Curso? Curso { get; set; }
 public Cuatrimestre? Cuatrimestre { get; set; }
 public CursoOferta? CursoOferta { get; set; }
 public SistemaEscolar.Models.Usuario? Estudiante { get; set; }

 public List<Evaluacion>? Evaluaciones { get; set; }
 }
}
