using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class CourseEnrolled
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? CourseId { get; set; }
        public string? Href { get; set; }

        public virtual Course? Course { get; set; }
        public virtual User? User { get; set; }
    }
}
