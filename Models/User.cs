using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class User
    {
        public User()
        {
            CourseEnrolleds = new HashSet<CourseEnrolled>();
            Tokens = new HashSet<Token>();
            UserCourses = new HashSet<UserCourse>();
        }

        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public string? TypeAuthentication { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public virtual ICollection<CourseEnrolled> CourseEnrolleds { get; set; }
        public virtual ICollection<Token> Tokens { get; set; }
        public virtual ICollection<UserCourse> UserCourses { get; set; }
    }
}
