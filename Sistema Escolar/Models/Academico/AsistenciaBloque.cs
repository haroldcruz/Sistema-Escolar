using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaEscolar.Models.Academico
{
 public class AsistenciaBloque
 {
 public int Id { get; set; }

 [Required]
 public int CalificacionBloqueId { get; set; }
 public CalificacionBloque? CalificacionBloque { get; set; }

 [Required]
 public int BloqueFechaId { get; set; }
 public BloqueFecha? BloqueFecha { get; set; }

 [Required]
 public bool Asistio { get; set; }
 }
}