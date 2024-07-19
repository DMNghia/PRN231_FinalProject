namespace FinalProject.Dto.Response
{
    public class GetCourseDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Href { get; set; }
        public UserDto Teacher { get; set; }
        public List<MoocDTO> Moocs { get; set; }
    }
}
