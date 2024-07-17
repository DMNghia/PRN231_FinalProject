using FinalProject.Models;

namespace FinalProject.Dto
{
    public class MoocDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? CourseId { get; set; }


        public List<LessonDTO> Lessons { get; set; }
    }
}