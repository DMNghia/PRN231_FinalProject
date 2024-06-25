using FinalProject.Dto;
using FinalProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                    Mooc = l.Mooc == null ? null : new MoocDTO
                    {
                        Id = l.Mooc.Id,
                        Name = l.Mooc.Name,
                        CourseId = l.Mooc.CourseId,
                        Description = l.Mooc.Description
                    }
                })
                .FirstOrDefault();

            if (lesson == null)
            {
                return NotFound();
            }

            return Ok(lesson);
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
               Mooc = l.Mooc == null ? null : new MoocDTO
               {
                   Id = l.Mooc.Id,
                   Name = l.Mooc.Name,
                   CourseId = l.Mooc.CourseId,
                   Description = l.Mooc.Description
               }
           })
           .ToList();

            return Ok(lessons);
        }
    }
}
