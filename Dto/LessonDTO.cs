namespace FinalProject.Dto
{
	public class LessonDTO
	{
		public int Id { get; set; }
		public string? Name { get; set; }
		public string? Description { get; set; }
		public int? MoocId { get; set; }
		public string? Href { get; set; }
		public string? VideoUrl { get; set; }
		public string? VideoTranscript { get; set; }
		public DateTime? CreateAt { get; set; }
		public DateTime? UpdateAt { get; set; }
	}
}
