using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class Category
    {
        public Category()
        {
            CourseCategories = new HashSet<CourseCategory>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Href { get; set; }

        public virtual ICollection<CourseCategory> CourseCategories { get; set; }
    }
}
