namespace FinalProject.Dto.Request
{
    public class AddCourseRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> Categories { get; set; }
        public string Image {  get; set; }
        public string Href { get; set; }
    }
}
