using FinalProject.Models;

namespace FinalProject.Dto
{
    public class CourseDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? Href { get; set; }
        public CategoryDTO Category { get; set; }
    }
}
