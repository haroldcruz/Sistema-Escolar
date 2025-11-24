using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.Models.Academico;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaEscolar.Pages.Instrumentos
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public IList<InstrumentoEvaluacion> Instrumentos { get; set; }

        public async Task OnGetAsync()
        {
            Instrumentos = await _context.InstrumentosEvaluacion.AsNoTracking().ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleLockAsync(int id)
        {
            var instrumento = await _context.InstrumentosEvaluacion.FindAsync(id);
            if (instrumento == null) return NotFound();
            instrumento.IsLocked = !instrumento.IsLocked;
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}