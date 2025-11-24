using System;
using System.Collections.Generic;

namespace SistemaEscolar.Models.Academico
{
 public class CursoOferta
 {
 public int Id { get; set; }
 public int CursoId { get; set; }
 public int CuatrimestreId { get; set; }
 public string NombreGrupo { get; set; } = "A"; // e.g., Grupo A, B
 public int? Capacidad { get; set; }
 public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

 // Navegación
 public Curso? Curso { get; set; }
 public Cuatrimestre? Cuatrimestre { get; set; }

 public List<CursoOfertaDocente> CursoOfertaDocentes { get; set; } = new();
 public List<Matricula> Matriculas { get; set; } = new();
 }
}
