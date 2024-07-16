using FinalProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinalProject.Pages
{
    public class UpdateCourseModel : PageModel
    {
        private readonly Prn231_FinalProjectContext _context;

        [BindProperty]
        public Course Course { get; set; }

        public UpdateCourseModel(Prn231_FinalProjectContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Course = await _context.Courses.FindAsync(id);

            if (Course == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
