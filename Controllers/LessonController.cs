using FinalProject.Dto;
using FinalProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit.Tnef;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly Prn231_FinalProjectContext _context;

        public LessonController(Prn231_FinalProjectContext context)
        {
            _context = context;
        }

        [HttpGet("Lesson-detail")]
        public IActionResult GetLessonByHref([FromQuery] string href)
        {
            var lesson = _context.Lessons
                .Include(l => l.Mooc)
                .Where(l => l.Href == href)
                .Select(l => new LessonDTO
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    Href = l.Href,
                    VideoUrl = l.VideoUrl,
                    MoocId = l.MoocId,
                    VideoTranscript = l.VideoTranscript,
                    CreateAt = l.CreateAt,
                    UpdateAt = l.UpdateAt,
                    //Mooc = l.Mooc == null ? null : new MoocDTO
                    //{
                    //    Id = l.Mooc.Id,
                    //    Name = l.Mooc.Name,
                    //    CourseId = l.Mooc.CourseId,
                    //    Description = l.Mooc.Description
                    //}
                })
                .FirstOrDefault();

            if (lesson == null)
            {
                return NotFound();
            }

            return Ok(lesson);
        }

        [HttpGet("CheckHrefExists")]
        public async Task<IActionResult> CheckHrefExists(string href)
        {
            var exists = await _context.Lessons.AnyAsync(l => l.Href == href);
            return Ok(exists);
        }

        [HttpGet("ListLesson")]
        public IActionResult getListLesson()
        {
            var lessons = _context.Lessons
           .Include(l => l.Mooc)
           .AsNoTracking()
           .Select(l => new LessonDTO
           {
               Id = l.Id,
               Name = l.Name,
               Description = l.Description,
               Href = l.Href,
               VideoUrl = l.VideoUrl,
               MoocId = l.MoocId,
               VideoTranscript = l.VideoTranscript,
               CreateAt = l.CreateAt,
               UpdateAt = l.UpdateAt,
               //Mooc = l.Mooc == null ? null : new MoocDTO
               //{
               //    Id = l.Mooc.Id,
               //    Name = l.Mooc.Name,
               //    CourseId = l.Mooc.CourseId,
               //    Description = l.Mooc.Description
               //}

           })
           .ToList();

            return Ok(lessons);
        }

        [HttpPost("AddLesson")]
        public async Task<IActionResult> AddLesson([FromBody] LessonDTO lessonDTO)
        {
            if (lessonDTO == null)
            {
                return BadRequest("Lesson data is null.");
            }


            var existingLesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Href == lessonDTO.Href);
            if (existingLesson != null)
            {
                return Conflict(new { message = "Lesson with the same Href already exists." });
            }

            try
            {
                var lesson = new Lesson
                {
                    Name = lessonDTO.Name,
                    Description = lessonDTO.Description,
                    Href = lessonDTO.Href,
                    VideoUrl = lessonDTO.VideoUrl,
                    MoocId = lessonDTO.MoocId,
                    VideoTranscript = lessonDTO.VideoTranscript,
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                };

                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Lesson added successfully", lessonId = lesson.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the lesson.", error = ex.Message });

            }


        }

        [HttpDelete("DeleteLesson/{id}")]
        public async Task<IActionResult> DeleteLessonById(int id)
        {
            try
            {

                var lesson = await _context.Lessons.FindAsync(id);
                if (lesson == null)
                {
                    return NotFound(new { message = "Lesson not found." });
                }

                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Lesson deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the lesson.", error = ex.Message });
            }
        }

        [HttpPut("EditLesson/{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromBody] LessonDTO lessonDTO)
        {
            if (lessonDTO == null)
            {
                return BadRequest("Lesson data is null.");
            }

            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound(new { message = "Lesson not found." });
            }


            var existingLesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Href == lessonDTO.Href && l.Id != id);
            if (existingLesson != null)
            {
                return Conflict(new { message = "Lesson with the same Href already exists." });
            }

            try
            {
                lesson.Name = lessonDTO.Name;
                lesson.Description = lessonDTO.Description;
                lesson.Href = lessonDTO.Href;
                lesson.VideoUrl = lessonDTO.VideoUrl;
                lesson.MoocId = lessonDTO.MoocId;
                lesson.VideoTranscript = lessonDTO.VideoTranscript;
                lesson.UpdateAt = DateTime.UtcNow;

                _context.Lessons.Update(lesson);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Lesson updated successfully", lessonId = lesson.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the lesson.", error = ex.Message });
            }
        }
    }
}
