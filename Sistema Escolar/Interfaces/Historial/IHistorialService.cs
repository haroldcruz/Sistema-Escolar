using SistemaEscolar.DTOs.Historial;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SistemaEscolar.Interfaces.Historial
{
 // Define las consultas de historial académico
 public interface IHistorialService
 {
 Task<EstudianteHistorialDTO> GetHistorialAsync(int estudianteId);
 Task<IEnumerable<EstudianteBusquedaDTO>> BuscarEstudiantesAsync(string termino); // nombre, identificacion
 Task<EstudianteHistorialAgrupadoDTO> GetHistorialAgrupadoAsync(int estudianteId); // agrupado por cuatrimestre
 Task<EstudianteHistorialAgrupadoDTO> GetHistorialAgrupadoFiltradoAsync(int estudianteId, DateTime? from, DateTime? to, IEnumerable<string>? cursos); // nuevo
 }
}
