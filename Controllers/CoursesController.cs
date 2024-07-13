using FinalProject.Dto;
using FinalProject.Models;
using FinalProject.Security;
using FinalProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {

        private readonly Prn231_FinalProjectContext _context;
        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
        public CourseController(Prn231_FinalProjectContext context)
        {
            _context = context;
        }

        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDTO>>> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.CourseCategories)
                .ThenInclude(cc => cc.Category)
                .Select(c => new CourseDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Image = c.Image,
                    Href = c.Href,
                    Category = c.CourseCategories.Any() ? new CategoryDTO
                    {
                        Id = c.CourseCategories.First().Category.Id,
                        Name = c.CourseCategories.First().Category.Name
                    } : null
                })
                .ToListAsync();

            return courses;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return course;
        }

        // POST: api/Courses
        [HttpPost("addCourse")]
        public async Task<ActionResult<CourseDTO>> AddCourse(CourseCreateDTO courseCreateDTO)
        {
            var category = await _context.Categories.FindAsync(courseCreateDTO.CategoryId);
            if (category == null)
            {
                return BadRequest("Invalid category ID.");
            }

            var course = new Course
            {
                Name = courseCreateDTO.Name,
                Description = courseCreateDTO.Description,
                Image = courseCreateDTO.Image,
                Href = courseCreateDTO.Href,
                CourseCategories = new List<CourseCategory>
                {
                    new CourseCategory
                    {
                        Category = category
                    }
                }
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var courseDTO = new CourseDTO
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Image = course.Image,
                Href = course.Href,
                Category = new CategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name
                }
            };

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, courseDTO);
        }

        [HttpPut("updateCourse/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, CourseUpdateDTO courseUpdateDTO)
        {
            var course = await _context.Courses
                .Include(c => c.CourseCategories)
                .ThenInclude(cc => cc.Category)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // Update course properties
            course.Name = courseUpdateDTO.Name;
            course.Description = courseUpdateDTO.Description;
            course.Image = courseUpdateDTO.Image;
            course.Href = courseUpdateDTO.Href;

            // Update category
            var category = await _context.Categories.FindAsync(courseUpdateDTO.CategoryId);
            if (category == null)
            {
                return BadRequest("Invalid category ID.");
            }

            // Update or add course category relationship
            var existingCourseCategory = course.CourseCategories.FirstOrDefault();
            if (existingCourseCategory != null)
            {
                existingCourseCategory.Category = category;
            }
            else
            {
                course.CourseCategories.Add(new CourseCategory
                {
                    Category = category,
                    CourseId = course.Id
                });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
    }
}
