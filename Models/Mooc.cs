using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class Mooc
    {
        public Mooc()
        {
            Lessons = new HashSet<Lesson>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? CourseId { get; set; }

        public virtual Course? Course { get; set; }
        public virtual ICollection<Lesson> Lessons { get; set; }
    }
}
