using FinalProject.Dto;
using FinalProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoocController : ControllerBase
    {
        private readonly Prn231_FinalProjectContext _context;

        public MoocController(Prn231_FinalProjectContext context)
        {
            _context = context;
        }

        [HttpGet("GetMoocs")]
        public async Task<IActionResult> GetMoocs()
        {
            var moocs = await _context.Moocs.Select(s=> new MoocDTO
            {
                Id = s.Id,
                Name = s.Name,
                CourseId = s.CourseId,
                Description = s.Description,

            }).ToListAsync();
            return Ok(moocs);
        }

        [HttpPost("AddMooc")]
        public async Task<IActionResult> AddMooc([FromQuery] string courseHref, [FromBody] MoocDTO moocDTO)
        {
            if (moocDTO == null)
            {
                return BadRequest("Mooc data is null.");
            }

            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Href == courseHref);

                if (course == null)
                {
                    return NotFound(new { message = "Course not found." });
                }

                var mooc = new Mooc
                {
                    Name = moocDTO.Name,
                    Description = moocDTO.Description,
                    CourseId = course.Id
                };

                _context.Moocs.Add(mooc);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Mooc added successfully", mooc = mooc.Name });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the mooc.", error = ex.Message });
            }
        }

        [HttpPut("EditMooc/{id}")]
        public async Task<IActionResult> EditMooc(int id, [FromBody] MoocDTO moocDTO)
        {
            if (moocDTO == null)
            {
                return BadRequest("Mooc data is null.");
            }

            try
            {
                var mooc = await _context.Moocs.FindAsync(id);
                if (mooc == null)
                {
                    return NotFound(new { message = "Mooc not found." });
                }

                
                mooc.Name = moocDTO.Name;
                mooc.Description = moocDTO.Description;

                _context.Moocs.Update(mooc);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Mooc updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the mooc.", error = ex.Message });
            }
        }

        [HttpDelete("DeleteMooc/{id}")]
        public async Task<IActionResult> DeleteMooc(int id)
        {
            try
            {
				var mooc = await _context.Moocs.Include(m => m.Lessons).FirstOrDefaultAsync(m => m.Id == id);
				if (mooc == null)
				{
					return NotFound(new { message = "Mooc not found." });
				}

				
				if (mooc.Lessons != null && mooc.Lessons.Any())
				{
					_context.Lessons.RemoveRange(mooc.Lessons);
				}

				_context.Moocs.Remove(mooc);
				await _context.SaveChangesAsync();

				return Ok(new { message = "Mooc and associated lessons deleted successfully" });
			}
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the mooc.", error = ex.Message });
            }
        }
    }
}





