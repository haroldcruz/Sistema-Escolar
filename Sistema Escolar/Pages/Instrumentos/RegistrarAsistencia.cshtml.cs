using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.Models.Academico;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaEscolar.Pages.Instrumentos
{
    public class RegistrarAsistenciaModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public RegistrarAsistenciaModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public List<Asistencia> Asistencias { get; set; }
        public InstrumentoEvaluacion Instrumento { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Instrumento = await _context.InstrumentosEvaluacion.FindAsync(id);
            if (Instrumento == null) return NotFound();
            if (Instrumento.IsLocked) return Forbid();

            // For demo: load matriculas (students) from Matricula table if exists
            Asistencias = await _context.Matriculas.Select(m => new Asistencia { MatriculaId = m.Id, Presente = false }).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Instrumento = await _context.InstrumentosEvaluacion.FindAsync(id);
            if (Instrumento == null) return NotFound();
            if (Instrumento.IsLocked) return Forbid();

            foreach (var a in Asistencias)
            {
                a.InstrumentoEvaluacionId = id;
                _context.Asistencias.Add(a);
            }
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}