using System;

namespace SistemaEscolar.DTOs.Estadisticas
{
 public class EstadisticaDTO
 {
 public int TotalEvaluaciones { get; set; }
 public double ParticipacionPromedio { get; set; }
 public int Aprobados { get; set; }
 public int Reprobados { get; set; }
 public double PorcentajeAprobados { get; set; }
 public double PorcentajeReprobados { get; set; }
 }
}