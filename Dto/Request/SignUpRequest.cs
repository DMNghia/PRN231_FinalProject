using System.ComponentModel.DataAnnotations;

namespace FinalProject.Dto.Request
{
    public class SignUpRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập tên của bạn")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Vui lòng điền email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Role không được để trống")]
        public string Role { get; set; }
    }
}
