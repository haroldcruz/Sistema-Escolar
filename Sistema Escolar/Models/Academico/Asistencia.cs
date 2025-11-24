namespace SistemaEscolar.Models.Academico
{
 public class Asistencia
 {
 public int Id { get; set; }
 public int InstrumentoEvaluacionId { get; set; }
 public required InstrumentoEvaluacion InstrumentoEvaluacion { get; set; }
 public int MatriculaId { get; set; }
 public bool Presente { get; set; }
 public required string Observacion { get; set; } = string.Empty;
 }
}