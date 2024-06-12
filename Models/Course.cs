using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class Course
    {
        public Course()
        {
            CourseCategories = new HashSet<CourseCategory>();
            CourseEnrolleds = new HashSet<CourseEnrolled>();
            Moocs = new HashSet<Mooc>();
            UserCourses = new HashSet<UserCourse>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? Href { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public virtual ICollection<CourseCategory> CourseCategories { get; set; }
        public virtual ICollection<CourseEnrolled> CourseEnrolleds { get; set; }
        public virtual ICollection<Mooc> Moocs { get; set; }
        public virtual ICollection<UserCourse> UserCourses { get; set; }
    }
}
