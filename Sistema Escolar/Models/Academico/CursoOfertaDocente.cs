namespace SistemaEscolar.Models.Academico
{
 public class CursoOfertaDocente
 {
 public int CursoOfertaId { get; set; }
 public int DocenteId { get; set; }

 public CursoOferta? CursoOferta { get; set; }
 public SistemaEscolar.Models.Usuario? Docente { get; set; }
 }
}
