using FinalProject.Constants;
using FinalProject.Dto;
using FinalProject.Models;
using FinalProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FinalProject.Pages
{
    public class Course_DetailModel : PageModel
    {
        private readonly Prn231_FinalProjectContext _context;

        public Course_DetailModel(Prn231_FinalProjectContext context)
        {
            _context = context;
        }

        public Course Course { get; private set; }

        public  IActionResult OnGet(int id)
        {
            ViewData["id"] = id;
            //Course = await _context.Courses
            //    .Include(c => c.CourseCategories)
            //    .ThenInclude(cc => cc.Category)
            //    .FirstOrDefaultAsync(c => c.Id == id);

            //if (Course == null)
            //{
            //    return NotFound();
            //}

            return Page();
        }
    }
}

