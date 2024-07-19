namespace FinalProject.Dto.Request
{
    public class AddCourseRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Categories { get; set; }
        public string Image {  get; set; }
        public string Href { get; set; }
    }
}
