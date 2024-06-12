using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class CourseCategory
    {
        public int Id { get; set; }
        public int? CourseId { get; set; }
        public int? CategoryId { get; set; }

        public virtual Category? Category { get; set; }
        public virtual Course? Course { get; set; }
    }
}
