using System;
using System.Collections.Generic;

namespace SistemaEscolar.Models.Academico
{
    public class InstrumentoEvaluacion
    {
        public int Id { get; set; }
        public required string Nombre { get; set; } = string.Empty;
        public required string Tipo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public bool IsLocked { get; set; } = false;

        public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
    }
}