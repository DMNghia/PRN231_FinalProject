using FinalProject.Models;

namespace FinalProject.Dto.Response
{
    public class GetAllCourseResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image {  get; set; }
        public string Href { get; set; }
        public UserDto Teacher { get; set; }
        public List<CategoryDto> Categories { get; set; }
    }
}
